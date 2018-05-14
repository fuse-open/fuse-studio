# Contributing

If you want to contribute to this repository, please first discuss the
changes you want to make with the repository's owners, for example via
an issue or on [Slack](https://fusecommunity.slack.com).

This project has a [code of conduct](#code-of-conduct). Please follow
it in all your interactions related to the project.

## Pull Request Process

1. Features should generally be pulled against the `master` branch.
   Bug fixes, depending on the urgency and risk, go either against an active
   release branch, or `master`.
2. Make sure the code follows our [coding style](Documentation/CodingStyle.md).
3. New features should have tests added, ensuring the feature does not
   silently break. Bug fixes should generally have regression tests added
   to ensure they're not reintroduced later.
4. New features must be documented.
5. If there are any functional changes, update CHANGELOG.md with a brief
   explanation.
6. Ensure that tests pass locally before sending a pull request.
7. Make sure your pull request passes the required CI (continuous
   integration). If there's a spurious error, retrigger the CI until your
   pull request passes, and feel free to file a ticket about the spurious
   test.
8. Make sure you follow up on any feedback to get your pull request merged.
9. If the pull request is against `master`, you may merge once the
   pull request has been approved by a project member and passed CI, if
   you have the permissions needed. Otherwise, ping the project member who
   approved the pull request. Pull requests against release branches should
   be done by the release manager (if you're unsure who this is, contact
   the project team).

## Code of Conduct

Please read and adhere to our [Code of Conduct](CODE_OF_CONDUCT.md)
