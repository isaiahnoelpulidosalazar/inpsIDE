# inpsIDE

A lightweight IDE for Windows made by inpsStudio, written in C# and Windows Forms on .NET 10.

Create, edit and run projects in several languages from one window. Each project type can bundle its own
language runtime, so there is nothing else to install.

## Features

- Project templates grouped by language (currently **PHP** and **Python**, including a PHP web app and a Python Flask web app)
- Code editor with line numbers and syntax highlighting (light theme)
- Project tree with create / delete / refresh
- Interactive console: output is streamed live and you can type input for programs that read from stdin
- Built-in web server support (PHP built-in server, Flask)
- Single standalone executable, no .NET installation required

## Download

Get the latest `inpsIDE-vX.Y.Z-win-x64.exe` from the
[Releases page](https://github.com/isaiahnoelpulidosalazar/inpsIDE/releases).
Each release includes a `.sha256` file so you can verify the download:

```powershell
Get-FileHash .\inpsIDE-vX.Y.Z-win-x64.exe -Algorithm SHA256
```

## Building from source

Requirements: Windows, the .NET 10 SDK.

```powershell
git clone https://github.com/isaiahnoelpulidosalazar/inpsIDE.git
cd inpsIDE
dotnet build -c Release
```

To bundle PHP, place the *PHP Thread Safe x64* Windows zip in `Resources\` before building
(see `Resources\README.txt`). The Python runtime is provided by the `inpsNuGet` package.

Releases are built by the GitHub Actions workflow in `.github/workflows/build_inpsIDE.yml`.

## Adding a language

Each language is one class in `Languages\`. Derive from `LanguageSupport`, describe its templates, runtime and
run commands, then add it to `LanguageRegistry`. See the comments in `Languages\LanguageSupport.cs`.

## Privacy policy

inpsIDE does not collect, store or transmit any personal data, and it contains no telemetry, analytics,
advertising or crash reporting.

Specifically:

- **No data leaves your computer because of inpsIDE itself.** The program does not transfer any information to other
  networked systems unless specifically requested by the user or the person installing or operating it.
- **Local data only.** inpsIDE stores one file on your computer: the list of recently opened projects
  (project names and folder paths) in `%AppData%\inpsIDE\recent.json`. Delete this file at any time to clear it.
  Your project files stay in the folders you choose.
- **Network access only when you ask for it.** The only situation where inpsIDE itself triggers network traffic is when
  you create a project whose template installs packages (for example the Python Flask template runs `pip install flask`,
  which downloads the package from the Python Package Index, PyPI). This is a direct result of your action, and your
  IP address and request are then handled by PyPI under its own policies.
- **Local web servers.** When you start a project's web server it listens on `127.0.0.1` (your own computer only), and
  inpsIDE opens your default browser to that local address.
- **Programs you run.** Code that you write and run from inpsIDE is executed with your user permissions and may do
  anything your code does, including accessing the network. That behaviour is controlled by you, not by inpsIDE.
- **Downloading releases.** Downloads from GitHub Releases are handled by GitHub under
  [GitHub's privacy statement](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement).

If this policy changes, the change will be described in the release notes and in this file.

## Contributing and issues

Bug reports and pull requests are welcome via
[GitHub Issues](https://github.com/isaiahnoelpulidosalazar/inpsIDE/issues).

## License

Released under the [MIT License](LICENSE). Bundled third-party runtimes keep their own licenses:
PHP is distributed under the [PHP License](https://www.php.net/license/), and Python under the
[Python Software Foundation License](https://docs.python.org/3/license.html).
