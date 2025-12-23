using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using FootballReport.Ui.UI.Services;

namespace FootballReport.Ui.UI.Views;

public sealed class ConfigEditorControl : UserControl
{
    private ComboBox _cmbCategory = null!;

    private Panel _listPanel = null!;
    private ListBox _lstValues = null!;
    private TextBox _txtValue = null!;
    private Button _btnAddValue = null!;
    private Button _btnReplaceValue = null!;
    private Button _btnRemoveValue = null!;

    private Panel _aliasPanel = null!;
    private DataGridView _gridAlias = null!;
    private TextBox _txtAliasRaw = null!;
    private TextBox _txtAliasCanon = null!;
    private Button _btnAddAlias = null!;
    private Button _btnReplaceAlias = null!;
    private Button _btnRemoveAlias = null!;

    private Button _btnApply = null!;
    private Button _btnCancel = null!;
    private Label _lblStatus = null!;

    private Dictionary<ConfigListKey, List<string>> _lists = new();
    private Dictionary<ConfigAliasKey, Dictionary<string, string>> _aliases = new();

    private sealed record CategoryItem(
        string Name,
        bool IsAlias,
        ConfigListKey? ListKey,
        ConfigAliasKey? AliasKey)
    {
        public override string ToString() => Name;
    }

    public ConfigEditorControl()
    {
        Dock = DockStyle.Fill;

        _cmbCategory = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Top,
            Height = 28
        };
        _cmbCategory.SelectedIndexChanged += (_, _) => RefreshCategoryView();

        _listPanel = BuildListPanel();
        _aliasPanel = BuildAliasPanel();

        _btnApply = new Button
        {
            Text = "Применить",
            Width = 120,
            Height = 30
        };
        _btnApply.Click += (_, _) => ApplyChanges();

        _btnCancel = new Button
        {
            Text = "Отменить",
            Width = 120,
            Height = 30
        };
        _btnCancel.Click += (_, _) => ReloadAll();

        _lblStatus = new Label
        {
            AutoSize = true,
            Text = "Загрузка конфигов...",
            Dock = DockStyle.Fill
        };

        var bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(8)
        };
        _btnApply.Location = new System.Drawing.Point(8, 5);
        _btnCancel.Location = new System.Drawing.Point(136, 5);
        _lblStatus.Location = new System.Drawing.Point(280, 10);
        bottomPanel.Controls.AddRange(new Control[] { _btnApply, _btnCancel, _lblStatus });

        Controls.Add(_listPanel);
        Controls.Add(_aliasPanel);
        Controls.Add(bottomPanel);
        Controls.Add(_cmbCategory);

        BuildCategories();
        ReloadAll();
    }

    private void BuildCategories()
    {
        _cmbCategory.Items.Clear();
        _cmbCategory.Items.AddRange(new object[]
        {
            new CategoryItem("Избранное — клубы", false, ConfigListKey.FavoritesClubs, null),
            new CategoryItem("Избранное — турниры", false, ConfigListKey.FavoritesCompetitions, null),
            new CategoryItem("Чёрный список — клубы", false, ConfigListKey.BlacklistClubs, null),
            new CategoryItem("Чёрный список — турниры", false, ConfigListKey.BlacklistCompetitions, null),
            new CategoryItem("Чёрный список — паттерны", false, ConfigListKey.BlacklistPatterns, null),
            new CategoryItem("Списки сравнения — клубы (alias)", true, null, ConfigAliasKey.AliasClubs),
            new CategoryItem("Списки сравнения — турниры (alias)", true, null, ConfigAliasKey.AliasCompetitions)
        });

        if (_cmbCategory.Items.Count > 0)
            _cmbCategory.SelectedIndex = 0;
    }

    private Panel BuildListPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8)
        };

        _lstValues = new ListBox
        {
            Dock = DockStyle.Fill
        };
        _lstValues.SelectedIndexChanged += (_, _) =>
        {
            if (_lstValues.SelectedItem is string s)
                _txtValue.Text = s;
        };

        _txtValue = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 26
        };

        _btnAddValue = new Button { Text = "Добавить", Width = 100, Height = 28 };
        _btnAddValue.Click += (_, _) => AddListValue();

        _btnReplaceValue = new Button { Text = "Заменить", Width = 100, Height = 28 };
        _btnReplaceValue.Click += (_, _) => ReplaceListValue();

        _btnRemoveValue = new Button { Text = "Удалить", Width = 100, Height = 28 };
        _btnRemoveValue.Click += (_, _) => RemoveListValue();

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 34,
            FlowDirection = FlowDirection.LeftToRight
        };
        buttonsPanel.Controls.AddRange(new Control[] { _btnAddValue, _btnReplaceValue, _btnRemoveValue });

        panel.Controls.Add(_lstValues);
        panel.Controls.Add(buttonsPanel);
        panel.Controls.Add(_txtValue);

        return panel;
    }

    private Panel BuildAliasPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8)
        };

        _gridAlias = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        _gridAlias.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Raw",
            HeaderText = "Исходное",
            DataPropertyName = "Raw",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _gridAlias.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Canonical",
            HeaderText = "Каноническое",
            DataPropertyName = "Canonical",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _gridAlias.SelectionChanged += (_, _) =>
        {
            if (_gridAlias.CurrentRow?.DataBoundItem is AliasRow row)
            {
                _txtAliasRaw.Text = row.Raw;
                _txtAliasCanon.Text = row.Canonical;
            }
        };

        _txtAliasRaw = new TextBox { PlaceholderText = "Исходное", Width = 180 };
        _txtAliasCanon = new TextBox { PlaceholderText = "Каноническое", Width = 180 };

        _btnAddAlias = new Button { Text = "Добавить", Width = 100, Height = 28 };
        _btnAddAlias.Click += (_, _) => AddAlias();

        _btnReplaceAlias = new Button { Text = "Заменить", Width = 100, Height = 28 };
        _btnReplaceAlias.Click += (_, _) => ReplaceAlias();

        _btnRemoveAlias = new Button { Text = "Удалить", Width = 100, Height = 28 };
        _btnRemoveAlias.Click += (_, _) => RemoveAlias();

        var editorPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 36,
            FlowDirection = FlowDirection.LeftToRight
        };
        editorPanel.Controls.AddRange(new Control[]
        {
            _txtAliasRaw, _txtAliasCanon, _btnAddAlias, _btnReplaceAlias, _btnRemoveAlias
        });

        panel.Controls.Add(_gridAlias);
        panel.Controls.Add(editorPanel);

        return panel;
    }

    private void ReloadAll()
    {
        try
        {
            _lists = ConfigFilesService.LoadLists();
            _aliases = ConfigFilesService.LoadAliases();
            _lblStatus.Text = "Загружено из файлов";
            RefreshCategoryView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось загрузить конфиги: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _lblStatus.Text = "Ошибка загрузки";
        }
    }

    private void RefreshCategoryView()
    {
        var item = _cmbCategory.SelectedItem as CategoryItem;
        if (item == null) return;

        var isAlias = item.IsAlias;
        _aliasPanel.Visible = isAlias;
        _listPanel.Visible = !isAlias;

        if (isAlias)
            RefreshAliasGrid();
        else
            RefreshListBox();
    }

    private void RefreshListBox()
    {
        var key = SelectedListKey();
        if (key == null) return;

        var list = _lists.TryGetValue(key.Value, out var l) ? l : new List<string>();
        _lstValues.DataSource = new BindingList<string>(list);
        _txtValue.Text = string.Empty;
    }

    private void RefreshAliasGrid()
    {
        var key = SelectedAliasKey();
        if (key == null) return;

        var map = _aliases.TryGetValue(key.Value, out var m) ? m : new Dictionary<string, string>();
        var rows = map.Select(kv => new AliasRow(kv.Key, kv.Value)).ToList();
        _gridAlias.DataSource = new BindingList<AliasRow>(rows);
        _txtAliasRaw.Text = string.Empty;
        _txtAliasCanon.Text = string.Empty;
    }

    private ConfigListKey? SelectedListKey()
        => (_cmbCategory.SelectedItem as CategoryItem)?.ListKey;

    private ConfigAliasKey? SelectedAliasKey()
        => (_cmbCategory.SelectedItem as CategoryItem)?.AliasKey;

    private void AddListValue()
    {
        var key = SelectedListKey();
        if (key == null) return;

        var value = (_txtValue.Text ?? string.Empty).Trim();
        if (value.Length == 0) return;

        var list = GetList(key.Value);
        if (list.Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase)))
            return;

        list.Add(value);
        RefreshListBox();
        _lblStatus.Text = "Изменения не сохранены";
    }

    private void ReplaceListValue()
    {
        var key = SelectedListKey();
        if (key == null) return;
        if (_lstValues.SelectedIndex < 0) return;

        var value = (_txtValue.Text ?? string.Empty).Trim();
        if (value.Length == 0) return;

        var list = GetList(key.Value);
        list[_lstValues.SelectedIndex] = value;
        RefreshListBox();
        _lblStatus.Text = "Изменения не сохранены";
    }

    private void RemoveListValue()
    {
        var key = SelectedListKey();
        if (key == null) return;
        if (_lstValues.SelectedIndex < 0) return;

        var list = GetList(key.Value);
        list.RemoveAt(_lstValues.SelectedIndex);
        RefreshListBox();
        _lblStatus.Text = "Изменения не сохранены";
    }

    private void AddAlias()
    {
        var key = SelectedAliasKey();
        if (key == null) return;

        var raw = (_txtAliasRaw.Text ?? string.Empty).Trim();
        var canon = (_txtAliasCanon.Text ?? string.Empty).Trim();
        if (raw.Length == 0 || canon.Length == 0) return;

        var map = GetAlias(key.Value);
        map[raw] = canon;
        RefreshAliasGrid();
        _lblStatus.Text = "Изменения не сохранены";
    }

    private void ReplaceAlias()
    {
        var key = SelectedAliasKey();
        if (key == null) return;
        if (_gridAlias.CurrentRow?.DataBoundItem is not AliasRow row) return;

        var rawNew = (_txtAliasRaw.Text ?? string.Empty).Trim();
        var canonNew = (_txtAliasCanon.Text ?? string.Empty).Trim();
        if (rawNew.Length == 0 || canonNew.Length == 0) return;

        var map = GetAlias(key.Value);
        map.Remove(row.Raw);
        map[rawNew] = canonNew;
        RefreshAliasGrid();
        _lblStatus.Text = "Изменения не сохранены";
    }

    private void RemoveAlias()
    {
        var key = SelectedAliasKey();
        if (key == null) return;
        if (_gridAlias.CurrentRow?.DataBoundItem is not AliasRow row) return;

        var map = GetAlias(key.Value);
        map.Remove(row.Raw);
        RefreshAliasGrid();
        _lblStatus.Text = "Изменения не сохранены";
    }

    private void ApplyChanges()
    {
        try
        {
            foreach (var kv in _lists)
                ConfigFilesService.SaveList(kv.Key, kv.Value);

            foreach (var kv in _aliases)
                ConfigFilesService.SaveAlias(kv.Key, kv.Value);

            _lblStatus.Text = "Сохранено";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось сохранить конфиги: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _lblStatus.Text = "Ошибка сохранения";
        }
    }

    private sealed record AliasRow(string Raw, string Canonical);

    private List<string> GetList(ConfigListKey key)
    {
        if (!_lists.TryGetValue(key, out var list))
        {
            list = new List<string>();
            _lists[key] = list;
        }

        return list;
    }

    private Dictionary<string, string> GetAlias(ConfigAliasKey key)
    {
        if (!_aliases.TryGetValue(key, out var map))
        {
            map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _aliases[key] = map;
        }

        return map;
    }
}
