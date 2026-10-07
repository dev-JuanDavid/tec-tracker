# Componentes de interfaz

Componentes Razor para MVC 5 con Tailwind del layout existente. El color principal es #6d5fb9.

## Estructura

- Inputs/_Field: etiqueta, ayuda, obligatoriedad y mensaje de error.
- Inputs/_Text, _Password, _TextArea, _Select, _Checkbox, _Hidden: editores.
- Forms/_Form: composición, token antiforgery, resumen de validación y acciones.
- Tables/_Table: columnas, texto, badges de estado, enlaces y estado vacío.
- Cards/_Metric: tarjeta reutilizable de indicador, valor y descripción.
- Charts/_Chart: panel de gráfica con título, descripción y estado accesible.
- Models/Components/UiComponents.cs: contratos y modelos de presentación.

## Formularios

Construye un FormComponent con ActionUrl generado por Url.Action e Inputs de tipo IFormInput.
Usa Name igual a la propiedad que espera la acción POST (también admite nombres como Direccion.Ciudad).
Pasa Value desde el modelo y Required según sus reglas. La validación del servidor sigue perteneciendo
a las DataAnnotations y al controlador; Required activa además la validación nativa del navegador.
Renderiza con:

    @Html.Partial("~/Views/Shared/Components/Forms/_Form.cshtml", form, new ViewDataDictionary(ViewData))

La copia de ViewData conserva ModelState y los valores enviados cuando el servidor devuelve errores.
No envuelvas el componente en otro formulario. Asigna un Id distinto si hay varios formularios.
Las contraseñas no se vuelven a mostrar. Checkbox incluye el campo oculto false de MVC.
Select conserva opciones deshabilitadas y grupos. Date usa yyyy-MM-dd.
Number admite Min, Max y Step; los valores y el model binding deben respetar la cultura de la aplicación.

Tipos iniciales: TextInput, EmailInput, PasswordInput, NumberInput, DateInput,
TextAreaInput, SelectInput, CheckboxInput y HiddenInput.

## Extensión (principio abierto/cerrado)

Para añadir un tipo con comportamiento propio, hereda de FormInput (o implementa IFormInput),
define EditorView apuntando a su parcial y sobrescribe Attributes si necesita atributos adicionales.
Para variaciones del texto, hereda de TextInput y sobrescribe InputType y/o Format.
No necesitas modificar _Form ni _Field ni añadir condicionales por tipo.
En el nuevo parcial utiliza InputAttributes.For(Html, Model) para conservar estilos de error y accesibilidad.
Usa los helpers MVC para mantener los nombres, el model binding y la prioridad de ModelState.
Las clases Tailwind deben aparecer completas y literales en las vistas.

## Tablas

TableComponent.From<T> recibe registros y TableColumn<T> con selectores que devuelven TableCell.
Las celdas admiten Text, Badge y Link. Usa Url.Action para los enlaces locales.
Razor codifica el contenido; evita Html.Raw para datos de usuarios.
La tabla tiene desplazamiento horizontal en pantallas pequeñas.
From materializa los registros: aplica paginación en el servidor antes de construir tablas grandes.
La tabla incluye búsqueda, ordenación y paginación de cliente mediante Scripts/components/table.js,
cargado una sola vez por el layout. EnableSearch y EnablePagination son true por defecto;
PageSize es 10. Cada tabla tiene un Id único y mantiene un estado independiente.
El buscador ignora tildes y mayúsculas y no busca en las celdas de acciones.
El pie utiliza selector, rango/total y cuatro flechas. La búsqueda reinicia la página actual.
Las vistas solo configuran TableComponent: no deben inicializar DataTables ni duplicar scripts.
Para tablas añadidas dinámicamente, llama a window.TecTables.initialize().
EnableExport activa exportación a Excel y PDF. Se exportan los resultados de la búsqueda
en todas las páginas, excluyendo las columnas de acciones. Las librerías se cargan al exportar.
FormComponent.UseGet permite formularios de consulta GET sin token en la URL.
SelectInput.DependsOn y OptionsUrl permiten cargar opciones desde GetUserByArea al cambiar un grupo.

## Ejemplo real

Views/Agent_Job/Create.cshtml y Edit.cshtml comparten Forms/_JobForm según idJob;
Views/Account/Register.cshtml y Views/Manage/EditUser.cshtml comparten Forms/_UserForm según UserId.
Views/Agent_Job/Index.cshtml y Views/Manage/UserList.cshtml definen tablas sin scripts propios.
Sus nombres de campos y acciones conservan el contrato del controlador existente.
Al añadir archivos C# o vistas, regístralos en Queue.csproj, que usa inclusiones explícitas.

