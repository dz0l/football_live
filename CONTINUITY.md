Goal (incl. success criteria):
- WinForms app runs the real fetch->dedup->filter->render pipeline via "Сформировать (сегодня)", shows accurate progress/log/KPIs, outputs GMT+3/+4/+5 HTML; Config tab lets users view/edit favorites/blacklists/aliases with load/apply/cancel.

Constraints/Assumptions:
- Workspace write; network enabled; Windows/PowerShell.
- Preserve blacklist priority and deterministic logic; keep UI steps tied to real actions.
- Maintain backup notes for critical config/UI changes.

Key decisions:
- Shared pipeline/service (src/ReportPipeline.cs) with config loader and path discovery; console delegates to it; UI references core project and uses shared paths.
- Config tab implemented via ConfigEditorControl + ConfigFilesService to edit favorites/blacklists/aliases JSON files with apply/cancel.

State:
- Pipeline and run UI wired and building successfully; config tab implemented and compiled.

Done:
- Ledger created; core code/configs/templates/UI reviewed.
- Added pipeline, config loader, shared paths; hooked console and UI run buttons/logs/KPIs/out folder; build dotnet build FootballReport.sln passes.
- Built Config editor UI (category dropdown, list/alias editors, apply/cancel) backed by ConfigFilesService; added backup note.

Now:
- Functional check/UX polish for config editing (validation, dedup handling) and integration with rest of UI/logging.

Next:
- Validate config edit flows with real files; consider preview/log integration and additional safeguards.

Open questions (UNCONFIRMED if needed):
- Preferred validation/dup handling for config entries.
- Any localization cleanup for existing garbled strings.

Working set (files/ids/commands):
- CONTINUITY.md; src/ReportPipeline.cs; src/ProjectPaths.cs; src/AppConfigLoader.cs; src/Program.cs; FootballReport.Ui/UI/Views/MainForm.*; FootballReport.Ui/UI/Services/*.cs; FootballReport.Ui/UI/Models/*.cs; configs/*; templates/*; backup.md.
