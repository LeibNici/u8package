using System.Drawing;
using System.Windows.Forms;

namespace Xinchuan.U8Bridge.Manager
{
    public partial class MainForm
    {
        private TextBox baseUrlTextBox;
        private TextBox apiKeyTextBox;
        private TextBox profileNameTextBox;
        private ComboBox modeComboBox;
        private TextBox userIdTextBox;
        private TextBox passwordTextBox;
        private TextBox loginDateTextBox;
        private TextBox serverTextBox;
        private CheckBox dbEnabledCheckBox;
        private TextBox dbServerTextBox;
        private TextBox dbNameTextBox;
        private TextBox dbUserTextBox;
        private TextBox dbPasswordTextBox;
        private CheckBox updateEnabledCheckBox;
        private ComboBox updateSourceTypeComboBox;
        private TextBox updateUrlTextBox;
        private TextBox updateCurrentVersionTextBox;
        private CheckBox updatePrereleaseCheckBox;
        private Button updateNowButton;
        private TextBox logTextBox;
        private Label statusLabel;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private void InitializeComponent()
        {
            Text = "信川 U8 Bridge 管理器";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(980, 680);
            Size = new Size(1100, 760);
            Font = new Font("Microsoft YaHei UI", 9F);
            FormClosing += OnFormClosing;
            Resize += OnFormResize;

            var root = CreateRootLayout();
            var configGroup = CreateConfigGroup();
            var actionPanel = CreateActionPanel();
            var logGroup = CreateLogGroup();

            root.Controls.Add(configGroup, 0, 0);
            root.Controls.Add(actionPanel, 0, 1);
            root.Controls.Add(logGroup, 0, 2);
            Controls.Add(root);
            InitializeTrayIcon();
        }

        private TableLayoutPanel CreateRootLayout()
        {
            var root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(12);
            root.RowCount = 3;
            root.ColumnCount = 1;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 450));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return root;
        }

        private GroupBox CreateConfigGroup()
        {
            var group = new GroupBox { Text = "Bridge 配置", Dock = DockStyle.Fill };
            var grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.Padding = new Padding(10);
            grid.ColumnCount = 4;
            grid.RowCount = 10;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 10; i++)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            }

            baseUrlTextBox = AddTextRow(grid, "监听地址", 0, 0);
            modeComboBox = AddComboRow(grid, "模式", 0, 2);
            apiKeyTextBox = AddTextRow(grid, "API Key", 1, 0);
            profileNameTextBox = AddTextRow(grid, "Profile", 1, 2);
            serverTextBox = AddTextRow(grid, "U8服务器", 2, 0);
            loginDateTextBox = AddTextRow(grid, "登录日期", 2, 2);
            userIdTextBox = AddTextRow(grid, "U8账号", 3, 0);
            passwordTextBox = AddTextRow(grid, "U8密码", 3, 2);
            passwordTextBox.UseSystemPasswordChar = true;
            dbEnabledCheckBox = AddCheckRow(grid, "启用读库", 4, 0);
            dbServerTextBox = AddTextRow(grid, "DB服务器", 4, 2);
            dbNameTextBox = AddTextRow(grid, "DB名称", 5, 0);
            dbUserTextBox = AddTextRow(grid, "DB用户", 5, 2);
            dbPasswordTextBox = AddTextRow(grid, "DB密码", 6, 0);
            dbPasswordTextBox.UseSystemPasswordChar = true;
            updateEnabledCheckBox = AddCheckRow(grid, "检测更新", 7, 0);
            updateSourceTypeComboBox = AddComboRow(grid, "更新来源", 7, 2, "githubRelease", "manifest");
            updateUrlTextBox = AddWideTextRow(grid, "检测地址", 8, 0);
            updateCurrentVersionTextBox = AddTextRow(grid, "当前版本", 9, 0);
            updateCurrentVersionTextBox.ReadOnly = true;
            updateCurrentVersionTextBox.BackColor = SystemColors.Control;
            updateCurrentVersionTextBox.TabStop = false;
            updatePrereleaseCheckBox = AddCheckRow(grid, "预发布", 9, 2);
            group.Controls.Add(grid);
            return group;
        }

        private FlowLayoutPanel CreateActionPanel()
        {
            var panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.Padding = new Padding(2, 8, 2, 2);

            panel.Controls.Add(CreateButton("加载配置", (s, e) => LoadConfigToForm()));
            panel.Controls.Add(CreateButton("保存配置", (s, e) => SaveConfigFromForm()));
            panel.Controls.Add(CreateButton("配置监听权限", (s, e) => ConfigureUrlAcl()));
            panel.Controls.Add(CreateButton("启动服务", (s, e) => StartBridge()));
            panel.Controls.Add(CreateButton("停止服务", (s, e) => StopBridge()));
            panel.Controls.Add(CreateButton("测试 Health", (s, e) => TestHealth()));
            panel.Controls.Add(CreateButton("测试 U8 登录", (s, e) => TestLogin()));
            panel.Controls.Add(CreateButton("检查更新", (s, e) => CheckUpdate()));
            updateNowButton = CreateButton("更新升级", (s, e) => InstallUpdate());
            updateNowButton.Enabled = false;
            panel.Controls.Add(updateNowButton);
            panel.Controls.Add(CreateButton("刷新日志", (s, e) => RefreshLog()));
            panel.Controls.Add(CreateButton("打开日志目录", (s, e) => OpenLogDirectory()));
            statusLabel = new Label { AutoSize = true, Padding = new Padding(18, 8, 0, 0) };
            panel.Controls.Add(statusLabel);
            return panel;
        }

        private GroupBox CreateLogGroup()
        {
            var group = new GroupBox { Text = "运行日志", Dock = DockStyle.Fill };
            logTextBox = new TextBox();
            logTextBox.Dock = DockStyle.Fill;
            logTextBox.Multiline = true;
            logTextBox.ScrollBars = ScrollBars.Both;
            logTextBox.Font = new Font("Consolas", 9F);
            group.Controls.Add(logTextBox);
            return group;
        }

        private TextBox AddTextRow(TableLayoutPanel grid, string label, int row, int col)
        {
            grid.Controls.Add(CreateLabel(label), col, row);
            var textBox = new TextBox { Dock = DockStyle.Fill };
            grid.Controls.Add(textBox, col + 1, row);
            return textBox;
        }

        private CheckBox AddCheckRow(TableLayoutPanel grid, string label, int row, int col)
        {
            grid.Controls.Add(CreateLabel(label), col, row);
            var checkBox = new CheckBox { Dock = DockStyle.Fill };
            grid.Controls.Add(checkBox, col + 1, row);
            return checkBox;
        }

        private TextBox AddWideTextRow(TableLayoutPanel grid, string label, int row, int col)
        {
            grid.Controls.Add(CreateLabel(label), col, row);
            var textBox = new TextBox { Dock = DockStyle.Fill };
            grid.Controls.Add(textBox, col + 1, row);
            grid.SetColumnSpan(textBox, 3);
            return textBox;
        }

        private ComboBox AddComboRow(TableLayoutPanel grid, string label, int row, int col, params string[] items)
        {
            grid.Controls.Add(CreateLabel(label), col, row);
            var comboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            comboBox.Items.AddRange(items.Length == 0 ? new object[] { "dryRun", "official" } : items);
            grid.Controls.Add(comboBox, col + 1, row);
            return comboBox;
        }

        private static Label CreateLabel(string text)
        {
            return new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
        }

        private static Button CreateButton(string text, System.EventHandler handler)
        {
            var button = new Button { Text = text, AutoSize = true, Height = 32, Margin = new Padding(4) };
            button.Click += handler;
            return button;
        }

        private void InitializeTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("打开管理器", null, (s, e) => RestoreFromTray());
            trayMenu.Items.Add("停止服务", null, (s, e) => StopBridge());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("退出", null, (s, e) => ExitApplication());

            trayIcon = new NotifyIcon();
            trayIcon.Text = "信川 U8 Bridge 管理器";
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, e) => RestoreFromTray();
        }
    }
}
