# Px.Utils contribution guidelines

## How to contribute
TBA

### Git workflow

New features should be developed in a separate branch `feature/<feature-name>` where `<feature-name>` is a short description of the feature. When the feature is ready, a pull request should be created to merge the feature branch into the `develop` branch. Similarly, bug fixes should be developed in a separate branch `bugfix/<bugfix-name>` and merged into `develop` via a pull request.

When the feature and bugfix goals for a release have been met, a pull request should be created to merge `develop` into `test`. Outside of the release cycle, the `test` branch should be kept in sync with `develop` by merging `develop` into `test` on a regular basis. Separate testing branch provides an environment for a "freeze" period before a release where only bugfixes are allowed. This allows for a more stable release and easier testing.

When the `test` branch has been tested and is ready for release, a pull request should be created to merge `test` into `master`. The master branch should always contain the latest release.

## Code quality

### Unit testing
Unit tests are written using [xUnit](https://xunit.github.io/) and can be found in the `Px.Utils.UnitTests` project. The goal is that every line of code is covered by at least one unit test if possible. The code author is responsible for checking the coverage before submitting a pull request. Pull requests that do not meet the coverage requirements will usually not be merged.

If you are making changes to a section of code that is not already covered by unit tests for some reason, coverage of that section will still be checked when the pull request is reviewed. It is also highly recommended that you add unit tests any time you come across a untested section that is somehow relevant to your changes, EVEN IF your changes will not directly affect that section of code.

### Code analysis
#### SonarCloud
TBA