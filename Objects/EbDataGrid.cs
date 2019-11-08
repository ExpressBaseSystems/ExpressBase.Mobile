using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public class EbDataGrid : EbControlContainer
    {
        public int Height { get; set; }

        public bool IsShowSerialNumber { get; set; }

        public bool IsColumnsResizable { get; set; }

        public bool AscendingOrder { get; set; }

        public bool IsSpecialContainer { get { return true; } set { } }

        public override List<EbControl> Controls { get; set; }

        public bool IsAddable { get; set; }

        public bool IsDisable { get; set; }

        public override bool Hidden { get; set; }

        public override string ToolIconHtml { get { return "<i class='fa fa-table'></i>"; } set { } }
    }

    public abstract class EbDGColumn : EbControl
    {
        public string Title { get; set; }

        public virtual string InputControlType { get; set; }

        public bool IsDisable { get; set; }

        public bool IsEditable { get; set; }

        public virtual int Width { get; set; }
    }

    public class EbDGStringColumn : EbDGColumn
    {
        public EbTextBox EbTextBox { get; set; }

        public EbDGStringColumn()
        {
            this.EbTextBox = new EbTextBox();
        }

        public TextMode TextMode
        {
            get { return this.EbTextBox.TextMode; }
            set { this.EbTextBox.TextMode = value; }
        }

        public int RowsVisible
        {
            get { return this.EbTextBox.RowsVisible; }
            set { this.EbTextBox.RowsVisible = value; }
        }

        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } }

        public override string InputControlType { get { return "EbTextBox"; } }

    }

    public class EbDGNumericColumn : EbDGColumn
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } }

        public override string InputControlType { get { return "EbNumeric"; } }

        public bool IsAggragate { get; set; }

        public bool AllowNegative { get; set; }
    }

    public class EbDGBooleanColumn : EbDGColumn
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.Boolean; } }

        public override string InputControlType { get { return "EbCheckBox"; } }
    }

    public class EbDGDateColumn : EbDGColumn
    {
        public EbDate EbDate { get; set; }

        public EbDGDateColumn()
        {
            this.EbDate = new EbDate();
        }

        public override string InputControlType { get { return "EbDate"; } }

        public override bool DoNotPersist
        {
            get { return this.EbDate.DoNotPersist; }
            set { this.EbDate.DoNotPersist = value; }
        }

        public bool IsNullable
        {
            get { return this.EbDate.IsNullable; }
            set { this.EbDate.IsNullable = value; }
        }
    }

    public class EbDGSimpleSelectColumn : EbDGColumn
    {
        public EbSimpleSelect EbSimpleSelect { get; set; }

        public EbDGSimpleSelectColumn()
        {
            this.EbSimpleSelect = new EbSimpleSelect();
        }

        public override string InputControlType { get { return "EbSimpleSelect"; } }

        public string DataSourceId
        {
            get { return this.EbSimpleSelect.DataSourceId; }
            set { this.EbSimpleSelect.DataSourceId = value; }
        }

        //public DVColumnCollection Columns
        //{
        //    get { return this.EbSimpleSelect.Columns; }
        //    set { this.EbSimpleSelect.Columns = value; }
        //}

        //public DVBaseColumn ValueMember
        //{
        //    get { return this.EbSimpleSelect.ValueMember; }
        //    set { this.EbSimpleSelect.ValueMember = value; }
        //}

        public List<EbSimpleSelectOption> Options
        {
            get { return this.EbSimpleSelect.Options; }
            set { this.EbSimpleSelect.Options = value; }
        }

        //public DVBaseColumn DisplayMember
        //{
        //    get { return this.EbSimpleSelect.DisplayMember; }
        //    set { this.EbSimpleSelect.DisplayMember = value; }
        //}

        public int Value
        {
            get { return this.EbSimpleSelect.Value; }
            set { this.EbSimpleSelect.Value = value; }
        }

        public override bool IsReadOnly
        {
            get { return this.EbSimpleSelect.IsReadOnly; }
            set { this.EbSimpleSelect.IsReadOnly = value; }
        }

        public bool IsDynamic
        {
            get { return this.EbSimpleSelect.IsDynamic; }
            set { this.EbSimpleSelect.IsDynamic = value; }
        }
    }

    public class EbDGBooleanSelectColumn : EbDGColumn
    {
        //public EbBooleanSelect EbBooleanSelect { get; set; }

        private EbDGSimpleSelectColumn EbDGSimpleSelectColumn { set; get; }

        public override string InputControlType { get { return "EbBooleanSelect"; } }
    }

    //public class EbDGUserControlColumn : EbDGColumn
    //{
    //    public EbUserControl EbUserControl { get; set; }

    //    public EbDGUserControlColumn()
    //    {
    //        this.EbUserControl = new EbUserControl();
    //    }

    //    public override string InputControlType { get { return "EbUserControl"; } }

    //    public List<EbControl> Columns
    //    {
    //        get { return this.EbUserControl.Controls; }
    //        set { this.EbUserControl.Controls = value; }
    //    }

    //    public override string RefId { get { return this.EbUserControl.RefId; } set { this.EbUserControl.RefId = value; } }

    //    public string ChildHtml { get; set; }
    //}

    //public class EbDGPowerSelectColumn : EbDGColumn
    //{
    //    public override string SetDisplayMemberJSfn { get { return this.EbPowerSelect.SetDisplayMemberJSfn; } set { } }

    //    private EbPowerSelect EbPowerSelect { get; set; }

    //    public EbButton AddButton { set; get; }

    //    public EbDGPowerSelectColumn()
    //    {
    //        this.EbPowerSelect = new EbPowerSelect();
    //        this.AddButton = new EbButton();
    //    }

    //    public string FormRefId { get { return this.AddButton.FormRefId; } set { this.AddButton.FormRefId = value; } }

    //    public bool IsInsertable { get; set; }

    //    public override int Width { get; set; }

    //    public string DataSourceId { get { return this.EbPowerSelect.DataSourceId; } set { this.EbPowerSelect.DataSourceId = value; } }

    //    public bool MultiSelect { get { return this.EbPowerSelect.MultiSelect; } set { this.EbPowerSelect.MultiSelect = value; } }

    //    public override bool Required { get { return this.EbPowerSelect.Required; } set { this.EbPowerSelect.Required = value; } }

    //    public int MaxLimit { get { return this.EbPowerSelect.MaxLimit; } set { this.EbPowerSelect.MaxLimit = value; } }

    //    public int MinLimit { get { return this.EbPowerSelect.MaxLimit; } set { this.EbPowerSelect.MinLimit = value; } }

    //    public DVColumnCollection Columns { get { return this.EbPowerSelect.Columns; } set { this.EbPowerSelect.Columns = value; } }

    //    public override string Name { get { return this.EbPowerSelect.Name; } set { this.EbPowerSelect.Name = value; } }

    //    public override string EbSid { get { return this.EbPowerSelect.EbSid; } set { this.EbPowerSelect.EbSid = value; } }

    //    public override EbDbTypes EbDbType { get { return this.EbPowerSelect.EbDbType; } set { this.EbPowerSelect.EbDbType = value; } }

    //    new public string EbSid_CtxId { get { return this.EbPowerSelect.EbSid_CtxId; } set { this.EbPowerSelect.EbSid_CtxId = value; } }

    //    public DVColumnCollection DisplayMembers { get { return this.EbPowerSelect.DisplayMembers; } set { this.EbPowerSelect.DisplayMembers = value; } }

    //    public int DropdownHeight { get { return this.EbPowerSelect.DropdownHeight; } set { this.EbPowerSelect.DropdownHeight = value; } }

    //    public int DropdownWidth { get { return this.EbPowerSelect.DropdownWidth; } set { this.EbPowerSelect.DropdownWidth = value; } }

    //    public DVBaseColumn ValueMember { get { return this.EbPowerSelect.ValueMember; } set { this.EbPowerSelect.ValueMember = value; } }

    //    public override string InputControlType { get { return "EbPowerSelect"; } }
    //}
}
