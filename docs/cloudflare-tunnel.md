# Cloudflare Tunnel Setup Guide

This guide walks you through exposing the Docker-hosted ReplayLab Web UI to the
internet via HTTPS using Cloudflare Tunnel — at zero cost.

Cloudflare Tunnel creates an encrypted tunnel between your Docker host and
Cloudflare's edge network. No port forwarding, no static IP, and no self-signed
certificates. Cloudflare handles TLS termination and issues a valid certificate
for your domain.

When you finish this guide, your Web UI will be reachable at a public HTTPS URL
backed by a persistent, named tunnel.

> **Precondition:** This guide assumes the Docker host is already running the
> Web UI and supporting services via `docker compose up` (set up in issues #141
> and #142). The Web app must be accessible at `http://localhost:5213`.

## 1. Prerequisites

- **Docker host** — running ReplayLab Web + Seq via `docker-compose.yml`. The
  Web app must respond on `http://localhost:5213`.
- **Cloudflare account** — free tier is sufficient. Sign up at
  [cloudflare.com](https://www.cloudflare.com/).
- **Domain managed by Cloudflare** — your domain must have its nameservers
  pointing to Cloudflare. If you are new to Cloudflare, add your domain from the
  dashboard and follow the nameserver change instructions (DNS propagation can
  take up to 24 hours, but is often much faster).
- **`curl`** — for verifying the tunnel is working.

## 2. Install cloudflared

`cloudflared` is the tunnel client daemon that runs on your Docker host. This
guide pins version `2024.12.2` for reproducibility.

**Ubuntu / Debian:**

```bash
# Download the pinned .deb package
curl -L -o cloudflared.deb \
  https://github.com/cloudflare/cloudflared/releases/download/2024.12.2/cloudflared-linux-amd64.deb

# Install it
sudo dpkg -i cloudflared.deb

# Verify the installation
cloudflared version
```

> If your host runs a different distribution, download the appropriate binary
> from the [cloudflared releases page](https://github.com/cloudflare/cloudflared/releases/tag/2024.12.2).
> Place the binary in `/usr/local/bin/` and ensure it is executable (`chmod +x`).

## 3. Authenticate cloudflared

Authenticate `cloudflared` with your Cloudflare account:

```bash
cloudflared tunnel login
```

This command:

1. Prints a URL to the terminal.
2. Opens your browser (or paste the URL manually).
3. Asks you to select the domain you want to authorize for tunnels.
4. Downloads a certificate file to `~/.cloudflared/cert.pem`.

Once the browser flow completes, the terminal reports success. The certificate
file lets `cloudflared` create and manage tunnels for the selected domain
without further interactive authentication.

> The certificate is scoped to the domain you selected. If you manage multiple
> domains in Cloudflare, you can repeat the login step later.

## 4. Create the tunnel

Create a named tunnel. The name is arbitrary — `replaylab` is used here:

```bash
cloudflared tunnel create replaylab
```

This command:

1. Registers the tunnel with Cloudflare's edge.
2. Outputs a **tunnel ID** (a UUID) — save this, you will need it later.
3. Creates a credentials file at
   `~/.cloudflared/<tunnel-id>.json`.

Example output:

```
Tunnel credentials written to /home/user/.cloudflared/abc12345-...json.
cloudflared chose this already created tunnel and no actions were taken for it.
```

Verify the tunnel exists:

```bash
cloudflared tunnel list
```

## 5. Configure the tunnel

Create the tunnel configuration file at `~/.cloudflared/config.yml`:

```bash
mkdir -p ~/.cloudflared
```

Then write the configuration. Replace `<your-domain>` with the domain or
subdomain you want to use (for example, `replaylab.example.com`):

```yaml
tunnel: <tunnel-id>
credentials-file: /home/<your-user>/.cloudflared/<tunnel-id>.json

ingress:
  - hostname: <your-domain>
    service: http://localhost:5213
  - service: http_status:404
```

> **What this does:**
>
> - `tunnel` and `credentials-file` link this config to the tunnel you created.
> - The first `ingress` rule routes requests for your domain to the Web UI
>   running on `http://localhost:5213`.
> - The last `ingress` rule (`http_status:404`) is a catch-all that returns HTTP
>   404 for any request that does not match a previous rule. **Always include
>   this fallback** — without it, your tunnel could be used as an open proxy.

About the ports:

- The Web UI is on `localhost:5213` (as defined in `docker-compose.yml`).
- `cloudflared` connects to this local port over plain HTTP. Cloudflare
  terminates TLS at the edge, so the local connection does not need HTTPS.

## 6. Create the DNS record

The tunnel needs a DNS record so Cloudflare can route traffic to it.

Open the Cloudflare dashboard, navigate to **DNS** → **Records**, and add a
record:

| Field | Value |
| --- | --- |
| **Type** | `CNAME` |
| **Name** | Your subdomain (e.g., `replaylab` for `replaylab.example.com`) |
| **Target** | `<tunnel-id>.cfargotunnel.com` |
| **Proxy status** | Proxied (orange cloud **must** be ON) |

> **Important:** The proxy (orange cloud) must be ON. Cloudflare Tunnel only
> works when traffic passes through Cloudflare's edge. If you turn the proxy
> off (grey cloud, DNS-only), the tunnel will not receive requests.

The target `<tunnel-id>.cfargotunnel.com` resolves to Cloudflare's edge network
and routes traffic to your tunnel regardless of your origin server's IP address.

## 7. Run the tunnel

Start the tunnel:

```bash
cloudflared tunnel run replaylab
```

You should see output similar to:

```
INF Initiating tunnel | tunnelID=<tunnel-id>
INF Connection registered connIndex=0 location=...
INF Each HA connection's tunnel IDs: ...
INF +-----------------------------------------------------------+
INF |  Your tunnel is running and routing traffic.              |
INF +-----------------------------------------------------------+
```

The tunnel runs in the foreground by default. Press `Ctrl+C` to stop it.

### Run as a systemd service (persistent)

For long-running deployments, install `cloudflared` as a systemd service so it
starts automatically on boot and restarts if it crashes:

```bash
sudo cloudflared service install
sudo systemctl enable cloudflared
sudo systemctl start cloudflared
```

Check the service status:

```bash
sudo systemctl status cloudflared
```

### Verify

From any machine with internet access:

```bash
curl -v https://<your-domain>
```

You should see an HTTP 200 response from the ReplayLab Web UI with a valid TLS
certificate issued by Cloudflare.

## 8. Verify HTTPS

The TLS certificate is issued by Cloudflare. Verify this:

```bash
curl -v https://<your-domain> 2>&1 | grep -E "issuer:|subject:"
```

You should see output like:

```
*  subject: CN=<your-domain>
*  issuer: C=US; O=Cloudflare, Inc.
```

If you see a self-signed certificate or the certificate does not mention
Cloudflare, the DNS record is likely set to DNS-only (grey cloud). Switch it to
Proxied (orange cloud) and wait a few minutes.

## 9. (Optional) Add Cloudflare Access for authentication

Without authentication, anyone who knows your domain can reach the Web UI.
Cloudflare Access (part of Cloudflare Zero Trust, available on the free plan)
lets you add an identity-aware access layer in front of the tunnel.

### GitHub OAuth

For a developer-oriented tool like ReplayLab, GitHub OAuth is a natural fit:

1. Open the [Cloudflare Zero Trust dashboard](https://one.dash.cloudflare.com/).
2. Navigate to **Access** → **Applications**.
3. Click **Add an application** → **Self-hosted**.
4. Enter the application name (e.g., `ReplayLab Web UI`).
5. Set the application domain to `<your-domain>`.
6. Under **Identity providers**, add **GitHub** (you may need to configure the
   GitHub OAuth app — Cloudflare's guided setup does this with one click).
7. Add a policy:
   - **Policy name:** `Allow developers`
   - **Action:** `Allow`
   - **Include:** select **Emails** and enter your email, or select **GitHub
     Organization** and enter your org name (e.g., `my-org`).
8. Click **Add application**.

Once configured, visiting `<your-domain>` displays a Cloudflare Access login
page. Users must authenticate with GitHub (or whichever provider you configured)
before reaching the Web UI.

### Email-based access

For simpler setups, skip the GitHub OAuth step and use a one-time PIN delivered
by email:

1. In the policy editor, add an **Include** rule: **Emails** → your email.
2. Cloudflare sends a one-time code to that address when anyone tries to access
   the page.

> Cloudflare Access is free for up to 50 users. See
> [Cloudflare Access pricing](https://www.cloudflare.com/plans/zero-trust-services/)
> for details.

## Troubleshooting

### "Unable to reach the origin service"

`cloudflared` cannot connect to `http://localhost:5213`. Check:

- Is the Docker Compose stack running? `docker compose ps`
- Is the Web app listening on port 5213? `curl http://localhost:5213`
- Is there a firewall blocking loopback connections? Rare, but check with
  `iptables -L` or `ufw status`.

### "Unknown certificate" or self-signed cert in browser

The DNS record is likely set to DNS-only (grey cloud). In the Cloudflare DNS
dashboard, make sure the Proxy status column shows **Proxied** (orange cloud).

### "ERR_SSL_VERSION_OR_CIPHER_MISMATCH"

This can happen while Cloudflare is provisioning the certificate for a new
domain. Wait 5-10 minutes and try again. Cloudflare issues Universal SSL
certificates automatically, but provisioning can be slow for newly added
domains.

### Tunnel starts but domain returns 404

Check the `ingress` rules in `~/.cloudflared/config.yml`:

- Is the `hostname` an exact match for your domain?
- Is the `service` pointed at `http://localhost:5213`?
- Is the catch-all rule (`service: http_status:404`) present at the end?

### cloudflared version mismatch

If you installed via a package manager and later want to pin a specific version,
download the binary directly from the
[releases page](https://github.com/cloudflare/cloudflared/releases). The version
used in this guide (`2024.12.2`) was current at the time of writing. Newer
versions should be compatible, but pinning avoids surprise breakage.

### "error="failed to dial to edge""

`cloudflared` cannot reach Cloudflare's edge network. Check:

- Is the host's outbound internet working? `curl https://cloudflare.com`
- Is a firewall blocking outbound QUIC (UDP 7844) or HTTPS (TCP 443)?
  `cloudflared` uses QUIC by default with a TCP fallback.

## What's next

- **Seq observability** — Once your Web UI is public, follow the Seq
  observability guide (planned, see
  [M15 deployment & observability milestone](milestones/m15-deployment-observability.md))
  to visualize structured logs from the replay engine, parsers, and senders.
- **Deploy workflow** — Automate the full deploy pipeline with GitHub Actions
  (see the `deploy-web.yml` workflow and
  [issue #142](https://github.com/sebastienwitz/replaylab/issues/142)).
- **Architecture** — [docs/architecture.md](architecture.md) explains how
  parsers, adapters, and hosting layers compose.
- **Getting started** — [docs/getting-started.md](getting-started.md) guides SDK
  consumers through building their own replay tool.
