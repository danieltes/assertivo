# Security Policy

## Supported Versions

| Version | Supported |
| ------- | --------- |
| 0.1.1   | ✅        |

Older versions do not receive security fixes. Please upgrade to the latest published release on [NuGet](https://www.nuget.org/packages/Assertivo).

## Reporting a Vulnerability

**Do not open a public GitHub issue for security vulnerabilities.**

Use GitHub's private disclosure feature instead:

1. Go to the [Security tab](../../security) of this repository.
2. Click **"Report a vulnerability"**.
3. Fill in the details (description, reproduction steps, affected versions, potential impact).

You will receive an acknowledgement within **5 business days**. We aim to triage and produce a fix within **30 days** of a confirmed report, coordinating a public disclosure (CVE if applicable) only after a patched release is available.

## Out of Scope

The following are **not** considered security vulnerabilities:

- Incorrect assertion logic or false positives/negatives in test results
- Missing assertion methods or API design feedback
- Issues only reproducible in unsupported versions

Please open a regular [GitHub issue](../../issues) for those.
