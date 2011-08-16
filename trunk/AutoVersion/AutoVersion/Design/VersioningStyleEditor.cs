using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using AutoVersion.Incrementors;
using AutoVersion.Incrementors.PostProcessors;

namespace AutoVersion.Design
{
    class VersioningStyleEditor : UITypeEditor
    {

        private VersioningUI versioningUI;

        public override bool IsDropDownResizable
        {
            get
            {
                return true;
            }
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (service != null)
                {
                    if (this.versioningUI == null)
                    {
                        this.versioningUI = new VersioningUI();
                    }
                    this.versioningUI.Start(service, value);
                    service.DropDownControl(this.versioningUI);
                    this.versioningUI.End();
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private class VersioningUI : Control
        {

            private IWindowsFormsEditorService edSvc;
            private DataGridView grid;
            private VersioningStyle value;
            private TypeConverter incrementorConverter;
            private TypeConverter processorConverter;

            public VersioningUI()
            {
                incrementorConverter = TypeDescriptor.GetConverter(typeof(BaseIncrementor));
                processorConverter = TypeDescriptor.GetConverter(typeof(BasePostProcessor));
                this.InitializeComponent();
            }

            private void InitializeComponent()
            {
                grid = new DataGridView();
                grid.RowHeadersWidth = 55;
                grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
                grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                grid.AllowUserToResizeRows = false;
                grid.AllowUserToAddRows = false;
                grid.AllowUserToDeleteRows = false;
                grid.AllowUserToOrderColumns = false;
                grid.EditMode = DataGridViewEditMode.EditOnEnter;
                grid.Dock = DockStyle.Fill;

                grid.Columns.Add(new DataGridViewTextBoxColumn() {
                    HeaderText = "Incrementor",
                    SortMode = DataGridViewColumnSortMode.NotSortable });
                grid.Columns.Add(new DataGridViewTextBoxColumn() {
                    HeaderText = "Action",
                    SortMode = DataGridViewColumnSortMode.NotSortable });
                grid.Columns.Add(new DataGridViewTextBoxColumn() {
                    HeaderText = "Post Processor",
                    SortMode = DataGridViewColumnSortMode.NotSortable });

                DataGridViewRow majorRow = new DataGridViewRow();
                majorRow.HeaderCell.Value = "Major";
                SetRowCells(majorRow);
                DataGridViewRow minorRow = new DataGridViewRow();
                minorRow.HeaderCell.Value = "Minor";
                SetRowCells(minorRow);
                DataGridViewRow buildRow = new DataGridViewRow();
                buildRow.HeaderCell.Value = "Build";
                SetRowCells(buildRow);
                DataGridViewRow releaseRow = new DataGridViewRow();
                releaseRow.HeaderCell.Value = "Release";
                SetRowCells(releaseRow);

                grid.Rows.AddRange(new DataGridViewRow[] { majorRow, minorRow, buildRow, releaseRow });

                grid.CellPainting += Grid_CellPainting;
                grid.CurrentCellDirtyStateChanged += Grid_CurrentCellDirtyStateChanged;

                Size = new Size(360, 140);
                Controls.Add(grid);
            }

            public void End()
            {
                edSvc = null;
                value.Major = (BaseIncrementor)incrementorConverter.ConvertFrom(grid.Rows[0].Cells[0].Value);
                value.MajorIncrementActionType = (BuildActionType) Enum.Parse(typeof(BuildActionType), (string)grid.Rows[0].Cells[1].Value);
                value.MajorProcessor = (BasePostProcessor)processorConverter.ConvertFrom(grid.Rows[0].Cells[2].Value);
                value.Minor = (BaseIncrementor)incrementorConverter.ConvertFrom(grid.Rows[1].Cells[0].Value);
                value.MinorIncrementActionType = (BuildActionType)Enum.Parse(typeof(BuildActionType), (string)grid.Rows[1].Cells[1].Value);
                value.MinorProcessor = (BasePostProcessor)processorConverter.ConvertFrom(grid.Rows[1].Cells[2].Value);
                value.Build = (BaseIncrementor)incrementorConverter.ConvertFrom(grid.Rows[2].Cells[0].Value);
                value.BuildIncrementActionType = (BuildActionType)Enum.Parse(typeof(BuildActionType), (string)grid.Rows[2].Cells[1].Value);
                value.BuildProcessor = (BasePostProcessor)processorConverter.ConvertFrom(grid.Rows[2].Cells[2].Value);
                value.Revision = (BaseIncrementor)incrementorConverter.ConvertFrom(grid.Rows[3].Cells[0].Value);
                value.RevisionIncrementActionType = (BuildActionType)Enum.Parse(typeof(BuildActionType), (string)grid.Rows[3].Cells[1].Value);
                value.RevisionProcessor = (BasePostProcessor)processorConverter.ConvertFrom(grid.Rows[3].Cells[2].Value);

                value = null;
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);

                grid.Focus();
            }

            private static void SetRowCells(DataGridViewRow row)
            {
                string[] incrementors = BuildVersionIncrementor.Instance.Incrementors.GetIncrementorNames();
                string[] processors = BuildVersionIncrementor.Instance.PostProcessors.GetPostProcessorNames();

                DataGridViewComboBoxCell incrementorsCell = new DataGridViewComboBoxCell();
                incrementorsCell.Items.AddRange(incrementors);
                incrementorsCell.Value = incrementors[0];
                row.Cells.Add(incrementorsCell);

                DataGridViewComboBoxCell buildActionsCell = new DataGridViewComboBoxCell();
                buildActionsCell.Items.AddRange(Enum.GetNames(typeof(BuildActionType)));
                buildActionsCell.Value = "Both";
                row.Cells.Add(buildActionsCell);

                DataGridViewComboBoxCell processorsCell = new DataGridViewComboBoxCell();
                processorsCell.Items.AddRange(processors);
                processorsCell.Value = processors[0];
                row.Cells.Add(processorsCell);
            }

            private void Grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
            {
                if (e.ColumnIndex == -1 && e.RowIndex > -1)
                {
                    e.PaintBackground(e.CellBounds, true);
                    using (SolidBrush br = new SolidBrush(Color.Black))
                    {
                        StringFormat sf = new StringFormat();
                        sf.LineAlignment = StringAlignment.Center;
                        Rectangle bounds = e.CellBounds;
                        bounds.Offset(5, 0);
                        e.Graphics.DrawString((string)grid.Rows[e.RowIndex].HeaderCell.Value,
                            e.CellStyle.Font, br, bounds, sf);
                    }
                    e.Handled = true;
                }
            }

            private void Grid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
            {
                if (grid.IsCurrentCellDirty) grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.value = value as VersioningStyle;
                if (this.value != null)
                {
                    grid.Rows[0].Cells[0].Value = incrementorConverter.ConvertToString(this.value.Major);
                    grid.Rows[0].Cells[1].Value = this.value.MajorIncrementActionType.ToString();
                    grid.Rows[0].Cells[2].Value = processorConverter.ConvertToString(this.value.MajorProcessor);
                    grid.Rows[1].Cells[0].Value = incrementorConverter.ConvertToString(this.value.Minor);
                    grid.Rows[1].Cells[1].Value = this.value.MinorIncrementActionType.ToString();
                    grid.Rows[1].Cells[2].Value = processorConverter.ConvertToString(this.value.MinorProcessor);
                    grid.Rows[2].Cells[0].Value = incrementorConverter.ConvertToString(this.value.Build);
                    grid.Rows[2].Cells[1].Value = this.value.BuildIncrementActionType.ToString();
                    grid.Rows[2].Cells[2].Value = processorConverter.ConvertToString(this.value.BuildProcessor);
                    grid.Rows[3].Cells[0].Value = incrementorConverter.ConvertToString(this.value.Revision);
                    grid.Rows[3].Cells[1].Value = this.value.RevisionIncrementActionType.ToString();
                    grid.Rows[3].Cells[2].Value = processorConverter.ConvertToString(this.value.RevisionProcessor);
                }
            }

        }

    }
}
