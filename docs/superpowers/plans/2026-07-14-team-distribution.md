# Team Distribution Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Package the Codex Usage Taskbar as a team-installable Codex plugin with a reusable source template and a self-contained Windows x64 release workflow.

**Architecture:** Keep the WPF solution in `src/` and tests in `tests/` as canonical source. Generate a copy of the complete project into the plugin's assets for Codex-guided reuse, validate the plugin against its manifest schema, and create a release ZIP outside Git using a PowerShell script.

**Tech Stack:** .NET 9 WPF, MSTest, PowerShell 5.1+, Codex plugin marketplace format, GitHub Releases.

## Global Constraints

- The widget never copies Codex credentials or modifies Codex binaries.
- The source template targets Windows and uses .NET 9 WPF; binary releases target only `win-x64` and are self-contained.
- `sync-plugin-template.ps1` must refuse to overwrite a user-selected non-empty destination.
- `package-release.ps1` must run the full solution tests and fail unless both application executables, a ZIP, and a SHA-256 file are present.
- `bin/`, `obj/`, and release artifacts are not committed; `plugins/codex-taskbar-usage-widget/assets/source-template/` is committed.
- Plugin marketplace policy is `AVAILABLE`; authentication policy is `ON_INSTALL`.

---

### Task 1: Establish distributable repository boundaries

**Files:**
- Create: `.gitignore`
- Modify: `README.md`
- Create: `docs/INSTALL.md`

**Interfaces:**
- Produces documented `dotnet test`, template-copy, marketplace, and release-install commands.
- Produces ignore rules consumed by all subsequent build and packaging steps.

- [ ] **Step 1: Write a failing documentation check**

Create `tests/DistributionLayout.Tests.ps1` with assertions that `.gitignore`, `docs/INSTALL.md`, `scripts/sync-plugin-template.ps1`, `scripts/package-release.ps1`, and the plugin manifest exist. Run it before adding those files and confirm it reports each missing path.

- [ ] **Step 2: Add repository rules and user documentation**

Add `.gitignore` entries for `bin/`, `obj/`, `dist/`, and `artifacts/`. Document the source, plugin, and binary workflows in Chinese in `README.md` and `docs/INSTALL.md`.

- [ ] **Step 3: Re-run the layout check**

Run: `powershell -ExecutionPolicy Bypass -File tests/DistributionLayout.Tests.ps1`

Expected: the check passes after Tasks 2 and 3 create the missing plugin and scripts.

- [ ] **Step 4: Commit**

Run: `git add .gitignore README.md docs/INSTALL.md tests/DistributionLayout.Tests.ps1 && git commit -m "docs: add distribution guidance"`

### Task 2: Create the marketplace-backed plugin

**Files:**
- Create: `.agents/plugins/marketplace.json`
- Create: `plugins/codex-taskbar-usage-widget/.codex-plugin/plugin.json`
- Create: `plugins/codex-taskbar-usage-widget/skills/codex-taskbar-usage-widget/SKILL.md`
- Create: `plugins/codex-taskbar-usage-widget/agents/openai.yaml`

**Interfaces:**
- Consumes canonical usage rules from `C:\Users\stan\.codex\skills\codex-taskbar-usage-widget\SKILL.md`.
- Produces a plugin named `codex-taskbar-usage-widget` discoverable from the repository marketplace.

- [ ] **Step 1: Make the layout check fail for an invalid plugin**

Extend `tests/DistributionLayout.Tests.ps1` to require the marketplace entry name, its `./plugins/codex-taskbar-usage-widget` source path, and the plugin manifest's matching name. Run the check before scaffolding and confirm failure.

- [ ] **Step 2: Scaffold and populate the plugin**

Run the plugin scaffold helper with the repository plugin directory and marketplace path. Set manifest metadata to version `0.1.0`, author `neystan`, email `2918360904@qq.com`, profile URL `https://github.com/neystan/`, skill path `./skills/`, and a Productivity interface. Copy the current Skill and `agents/openai.yaml`, then add template-first guidance that points to `assets/source-template`.

- [ ] **Step 3: Validate plugin and layout**

Run: `python C:\Users\stan\.codex\skills\.system\plugin-creator\scripts\validate_plugin.py plugins/codex-taskbar-usage-widget`

Run: `powershell -ExecutionPolicy Bypass -File tests/DistributionLayout.Tests.ps1`

Expected: both commands exit 0.

- [ ] **Step 4: Commit**

Run: `git add .agents plugins tests/DistributionLayout.Tests.ps1 && git commit -m "feat: add team marketplace plugin"`

### Task 3: Add safe source-template synchronization

**Files:**
- Create: `scripts/sync-plugin-template.ps1`
- Modify: `tests/DistributionLayout.Tests.ps1`
- Generate (tracked): `plugins/codex-taskbar-usage-widget/assets/source-template/`

**Interfaces:**
- `scripts/sync-plugin-template.ps1 [-Destination <path>]` copies `CodexUsageTaskbar.sln`, `src/`, `tests/`, `scripts/install-autostart.ps1`, and `README.md`.
- With no `Destination`, it refreshes the ignored plugin asset template; with `Destination`, it errors if the destination contains files.

- [ ] **Step 1: Write failing script behavior checks**

Extend the PowerShell check to invoke `sync-plugin-template.ps1 -Destination <temporary non-empty directory>` and require a non-zero exit; invoke it with an empty temporary directory and require the solution, source project, test project, and installer script in the result. Confirm failure before the script exists.

- [ ] **Step 2: Implement the copy script**

Use `Copy-Item -Recurse -Force` only after checking that an explicit destination is empty. Exclude `bin` and `obj` by copying only named source directories and files. Use a staging directory for the default plugin asset refresh, then replace only the generated asset directory.

- [ ] **Step 3: Run behavior and solution tests**

Run: `powershell -ExecutionPolicy Bypass -File tests/DistributionLayout.Tests.ps1`

Run: `dotnet test CodexUsageTaskbar.sln`

Expected: both exit 0.

- [ ] **Step 4: Commit**

Run: `git add scripts/sync-plugin-template.ps1 tests/DistributionLayout.Tests.ps1 && git commit -m "feat: add reusable source template"`

### Task 4: Build self-contained release artifacts

**Files:**
- Create: `scripts/package-release.ps1`
- Modify: `tests/DistributionLayout.Tests.ps1`
- Generate (ignored): `artifacts/CodexUsageTaskbar-win-x64-<version>.zip`
- Generate (ignored): `artifacts/CodexUsageTaskbar-win-x64-<version>.zip.sha256`

**Interfaces:**
- `scripts/package-release.ps1 [-Version <version>]` runs `dotnet test`, publishes both projects with `-r win-x64 --self-contained true`, creates a ZIP, and writes a UTF-8 SHA-256 manifest.
- The ZIP root contains `CodexUsageTaskbar.exe`, `CodexUsageTaskbar.Launcher.exe`, their runtime files, `install-autostart.ps1`, `README.md`, and `INSTALL.md`.

- [ ] **Step 1: Write failing package-output checks**

Extend the test script to invoke `package-release.ps1 -Version 0.1.0-test` in a temporary artifact directory and require the ZIP, checksum file, and both executable names in the ZIP. Confirm it fails before the script exists.

- [ ] **Step 2: Implement package-release.ps1**

Set `$ErrorActionPreference = 'Stop'`; validate semantic version input; run the full solution tests; publish the widget and launcher into one staging directory using `--self-contained true`; copy the installer and documentation; check both executables with `Test-Path`; use `Compress-Archive`; and write `Get-FileHash -Algorithm SHA256` output to `<zip>.sha256` using UTF-8.

- [ ] **Step 3: Run all validation**

Run: `powershell -ExecutionPolicy Bypass -File tests/DistributionLayout.Tests.ps1`

Run: `dotnet test CodexUsageTaskbar.sln`

Run: `python C:\Users\stan\.codex\skills\.system\plugin-creator\scripts\validate_plugin.py plugins/codex-taskbar-usage-widget`

Expected: every command exits 0; the release ZIP and checksum exist under ignored `artifacts/`.

- [ ] **Step 4: Commit**

Run: `git add scripts/package-release.ps1 tests/DistributionLayout.Tests.ps1 README.md docs/INSTALL.md .gitignore && git commit -m "feat: package self-contained release"`

### Task 5: Final pre-handoff verification

**Files:**
- Verify: repository tree, Git status, plugin manifest, source template, and release artifacts.

- [ ] **Step 1: Check source versus generated template**

Run the synchronization script, then verify the template includes `CodexUsageTaskbar.sln`, both `src` projects, `tests/CodexUsageTaskbar.Tests`, `scripts/install-autostart.ps1`, and `README.md`.

- [ ] **Step 2: Check repository contents**

Run: `git status --short`

Expected: source, source-template, documentation, plugin definitions, scripts, and tests are committed; `bin`, `obj`, and `artifacts` do not appear as tracked changes.

- [ ] **Step 3: Record release evidence**

Run `Get-ChildItem artifacts` and `Get-FileHash artifacts/*.zip -Algorithm SHA256`. Record the exact ZIP size and checksum for the later GitHub Release upload.
