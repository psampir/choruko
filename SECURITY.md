# Security policy

## Supported versions

Security fixes are applied to the latest release on `main`. Older versions are not
maintained; upgrade to the newest release.

## Reporting a vulnerability

**Do not open a public issue.** Use GitHub's private vulnerability reporting:

1. Go to the repository's **Security** tab.
2. Choose **Report a vulnerability**.
3. Describe the issue with enough detail to reproduce it.

If private reporting is unavailable, contact the maintainer
[@psampir](https://github.com/psampir) through GitHub and ask for a private channel.

Please include:

- the affected version and how it was deployed (Docker Compose, reverse proxy),
- steps to reproduce,
- impact and any proof of concept,
- relevant logs with secrets removed.

## Scope

Choruko is a self-hosted application. The project is responsible for the application
code; the security of a particular deployment (TLS, firewall, database credentials,
backups, host hardening) is the operator's responsibility.

In scope:

- authentication and authorization bypasses,
- injection, cross-site scripting and request forgery in the application,
- privilege escalation between household roles,
- exposure of other users' data,
- denial of service through normal API use.

Out of scope:

- vulnerabilities in unmodified third-party dependencies (report them upstream, but
  a heads-up is welcome),
- misconfiguration of a deployment, for example a missing TLS terminator or an open
  database port,
- issues that require an already compromised host or browser.

## Disclosure

Please give us reasonable time to fix the issue before publishing details. We will
credit reporters in the release notes unless they prefer to stay anonymous.
