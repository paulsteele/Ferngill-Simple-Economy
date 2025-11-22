# Contributing to Ferngill Simple Economy
## Bug Reporting

Please report bugs at https://github.com/paulsteele/Ferngill-Supply-And-Demand/issues and fill in as much of the template as possible.

## Localization

Translation files are located in:
* `FerngillSimpleEconomy/i18n/`. (Most resources)
* `FerngillSimpleEconomy/assets/mail/i18n/` (The intro mail)

The wiki has a short guide for how translations work in SMAPI https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Translation#Tips_for_translators

### Pull Request Process

In general the only "requirement" for submitting a PR is that the solution builds, and the existing tests still pass.

You can run them with `dotnet test`. 

I'm honored that people want to contribute to this project and would like to merge as many contributions as possible. At the same time this is a hobby project for me and I have limited time to adapt PRs into a code style I can support. PRs that include some of the following will help reduce the time it takes to integrate them:
* Follow existing code style
* Following architectural patterns (dependency injection, etc) already in use
* Include tests for new functionality

All that said, if you have an idea or a partial implementation please create a draft PR and I'll do my best to support you in getting it merged.
