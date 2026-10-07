using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Queue.Models.Components
{
    public interface IFormInput
    {
        string Name { get; }
        string Label { get; }
        string Help { get; }
        object Value { get; }
        bool Required { get; }
        bool ShowLabel { get; }
        bool Hidden { get; }
        string EditorView { get; }
        IDictionary<string, object> Attributes();
    }

    public abstract class FormInput : IFormInput
    {
        protected const string Root = "~/Views/Shared/Components/Inputs/";
        public string Name { get; set; }
        public string Label { get; set; }
        public string Help { get; set; }
        public object Value { get; set; }
        public bool Required { get; set; }
        public string Placeholder { get; set; }
        public string Autocomplete { get; set; }
        public string Id { get; set; }
        public virtual bool ShowLabel { get { return true; } }
        public virtual bool Hidden { get { return false; } }
        public abstract string EditorView { get; }
        public virtual IDictionary<string, object> Attributes()
        {
            var attributes = new Dictionary<string, object>
            {
                { "class", "w-full rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-800 shadow-sm transition placeholder:text-slate-400 focus:border-[#6d5fb9] focus:outline-none focus:ring-4 focus:ring-[#6d5fb9]/10" }
            };
            if (!string.IsNullOrEmpty(Placeholder)) attributes["placeholder"] = Placeholder;
            if (!string.IsNullOrEmpty(Autocomplete)) attributes["autocomplete"] = Autocomplete;
            if (!string.IsNullOrEmpty(Id)) attributes["id"] = Id;
            if (Required)
            {
                attributes["required"] = "required";
                attributes["aria-required"] = "true";
            }
            return attributes;
        }
    }

    public class TextInput : FormInput
    {
        public virtual string Format { get { return null; } }
        public virtual string InputType { get { return "text"; } }
        public override string EditorView { get { return Root + "_Text.cshtml"; } }
        public override IDictionary<string, object> Attributes()
        {
            var attributes = base.Attributes();
            attributes["type"] = InputType;
            return attributes;
        }
    }
    public class EmailInput : TextInput { public override string InputType { get { return "email"; } } }
    public class PasswordInput : TextInput
    {
        public override string InputType { get { return "password"; } }
        public override string EditorView { get { return Root + "_Password.cshtml"; } }
    }
    public class DateInput : TextInput
    {
        public override string Format { get { return "{0:yyyy-MM-dd}"; } }
        public override string InputType { get { return "date"; } }
    }
    public class NumberInput : TextInput
    {
        public string Min { get; set; }
        public string Max { get; set; }
        public string Step { get; set; } = "any";
        public override string InputType { get { return "number"; } }
        public override IDictionary<string, object> Attributes()
        {
            var attributes = base.Attributes();
            if (Min != null) attributes["min"] = Min;
            if (Max != null) attributes["max"] = Max;
            attributes["step"] = Step;
            return attributes;
        }
    }
    public class TextAreaInput : FormInput
    {
        public int Rows { get; set; } = 4;
        public override string EditorView { get { return Root + "_TextArea.cshtml"; } }
    }
    public class SelectInput : FormInput
    {
        public string DependsOn { get; set; }
        public string OptionsUrl { get; set; }
        public override IDictionary<string, object> Attributes()
        {
            var attributes = base.Attributes();
            if (!string.IsNullOrWhiteSpace(DependsOn) && !string.IsNullOrWhiteSpace(OptionsUrl))
            {
                attributes["data-depends-on"] = DependsOn;
                attributes["data-options-url"] = OptionsUrl;
            }
            return attributes;
        }
        public IEnumerable<SelectListItem> Options { get; set; } = new List<SelectListItem>();
        public string EmptyLabel { get; set; } = "Selecciona una opción";
        public override string EditorView { get { return Root + "_Select.cshtml"; } }
    }
    public class CheckboxInput : FormInput
    {
        public override bool ShowLabel { get { return false; } }
        public override string EditorView { get { return Root + "_Checkbox.cshtml"; } }
        public override IDictionary<string, object> Attributes()
        {
            var attributes = base.Attributes();
            attributes["class"] = "h-4 w-4 rounded border-slate-300 accent-[#6d5fb9] focus:ring-[#6d5fb9]";
            return attributes;
        }
    }
    public class FileInput : FormInput
    {
        public string Accept { get; set; }
        public override string EditorView { get { return Root + "_File.cshtml"; } }
        public override IDictionary<string, object> Attributes()
        {
            var attributes = base.Attributes();
            attributes["type"] = "file";
            if (!string.IsNullOrWhiteSpace(Accept)) attributes["accept"] = Accept;
            attributes["class"] = "block w-full cursor-pointer rounded-xl border border-dashed border-[#6d5fb9]/40 bg-[#6d5fb9]/5 p-4 text-sm text-slate-600 file:mr-3 file:cursor-pointer file:rounded-lg file:border-0 file:bg-[#6d5fb9] file:px-4 file:py-2 file:font-medium file:text-white focus:outline-none focus:ring-4 focus:ring-[#6d5fb9]/20";
            return attributes;
        }
    }
    public class HiddenInput : FormInput
    {
        public override bool ShowLabel { get { return false; } }
        public override bool Hidden { get { return true; } }
        public override string EditorView { get { return Root + "_Hidden.cshtml"; } }
    }

    public static class InputAttributes
    {
        public static IDictionary<string, object> For(HtmlHelper html, IFormInput input)
        {
            var attributes = input.Attributes();
            var name = html.ViewData.TemplateInfo.GetFullHtmlFieldName(input.Name);
            var id = HtmlHelper.GenerateIdFromName(name);
            attributes["aria-describedby"] = id + "_error" + (string.IsNullOrWhiteSpace(input.Help) ? "" : " " + id + "_help");
            ModelState state;
            if (html.ViewData.ModelState.TryGetValue(name, out state) && state.Errors.Count > 0)
            {
                attributes["aria-invalid"] = "true";
                attributes["class"] = Convert.ToString(attributes["class"]) + " border-rose-400 bg-rose-50/30";
            }
            return attributes;
        }
    }

    public class FormComponent
    {
        public string Id { get; set; } = "component-form";
        public string Title { get; set; }
        public string Description { get; set; }
        public string ActionUrl { get; set; }
        public string CancelUrl { get; set; }
        public string SubmitLabel { get; set; } = "Guardar cambios";
        public bool UploadFiles { get; set; }
        public bool SingleColumn { get; set; }
        public bool UseGet { get; set; }
        public IList<IFormInput> Inputs { get; set; } = new List<IFormInput>();
    }

    public class MetricComponent
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
    public class DetailField
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }
    public class DetailComponent
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IList<DetailField> Fields { get; set; } = new List<DetailField>();
        public IList<TableCell> Actions { get; set; } = new List<TableCell>();
    }
    public class ChartComponent
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public enum CellKind { Text, Badge, Link }
    public class TableCell
    {
        public string Value { get; set; }
        public CellKind Kind { get; set; }
        public bool Positive { get; set; }
        public string Url { get; set; }
        public static TableCell Text(object value) { return new TableCell { Value = Convert.ToString(value, CultureInfo.CurrentCulture) }; }
        public static TableCell Badge(string value, bool positive) { return new TableCell { Value = value, Kind = CellKind.Badge, Positive = positive }; }
        public static TableCell Link(string label, string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/") || url.StartsWith("//") || url.Contains("\\"))
                throw new ArgumentException("Las acciones de la tabla requieren una ruta local generada con Url.Action.", "url");
            return new TableCell { Value = label, Kind = CellKind.Link, Url = url };
        }
    }
    public class TableColumn<T>
    {
        public string Title { get; set; }
        public Func<T, TableCell> Cell { get; set; }
    }
    public class TableComponent
    {
        public string Id { get; set; } = "table-" + Guid.NewGuid().ToString("N");
        public bool EnableSearch { get; set; } = true;
        public bool EnablePagination { get; set; } = true;
        public bool EnableExport { get; set; }
        public int PageSize { get; set; } = 10;
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreateUrl { get; set; }
        public string CreateLabel { get; set; } = "Nuevo registro";
        public IList<TableCell> Actions { get; set; } = new List<TableCell>();
        public string EmptyMessage { get; set; } = "Todavía no hay registros.";
        public IList<string> Headers { get; set; } = new List<string>();
        public IList<IList<TableCell>> Rows { get; set; } = new List<IList<TableCell>>();
        public static TableComponent From<T>(IEnumerable<T> items, params TableColumn<T>[] columns)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (columns == null || columns.Length == 0 || columns.Any(c => c == null || c.Cell == null))
                throw new ArgumentException("Define al menos una columna con su selector.", "columns");
            return new TableComponent
            {
                Headers = columns.Select(c => c.Title).ToList(),
                Rows = items.Select(item => (IList<TableCell>)columns.Select(c => c.Cell(item) ?? TableCell.Text(null)).ToList()).ToList()
            };
        }
    }
}
