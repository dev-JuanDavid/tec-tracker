# Reglas de diseño del aplicativo

Estas reglas se aplican al actualizar cualquier vista.

1. Todo texto visible para el usuario debe estar en español: títulos, etiquetas, acciones, validaciones, estados vacíos, errores y textos de gráficas. El inglés se admite para terminología propia cuando traducirla cause confusión.
2. Reutilizar primero los componentes de Queue/Views/Shared/Components y sus modelos de Queue/Models/Components.
3. Extraer nuevos microcomponentes cuando una pieza visual pueda reutilizarse. Los inputs deben seguir IFormInput y permitir extensiones sin modificar el formulario genérico.
4. Usar Tailwind y el color principal #6d5fb9, con fondos blancos, tonos slate, bordes suaves y contraste legible.
5. Mantener diseños adaptables, etiquetas asociadas, foco visible y mensajes de error accesibles. El color no debe ser la única forma de transmitir un estado.
6. Conservar rutas, nombres de campos, permisos y validación del servidor al rediseñar vistas.
7. Guardar archivos en UTF-8; las vistas Razor usan BOM para evitar problemas de tildes en ASP.NET clásico.
8. Las gráficas usan datos reales, etiquetas en español y estados vacíos o de error. No mostrar controles sin función.
9. Evitar repetir dependencias que carga el layout. Registrar nuevos archivos en Queue.csproj.
10. Migrar vista por vista. Referencia inicial: Cargos y el panel de inicio.
11. Crear y editar deben compartir el mismo formulario cuando sus campos sean similares. Determinar el modo mediante un identificador o parámetro explícito y mantener únicamente las diferencias necesarias.
12. En tablas, el buscador se sitúa arriba a la izquierda. El pie muestra, en este orden: selector «Elementos por página», rango actual y total («1 - 7 de 7»), y flechas de primera página, anterior, siguiente y última. No mostrar botones numéricos de página. Mantener el orden al adaptar el pie a pantallas pequeñas.
13. El comportamiento compartido pertenece al componente: búsqueda, paginación, ordenación, estados y estilos. Las vistas configuran modelos y opciones; no repiten inicializadores ni dependencias por pantalla.

## Componentes disponibles

Inputs, formularios, tablas, tarjetas de métricas (_Metric) y paneles de gráficas (_Chart).
Ver Queue/Views/Shared/Components/README.md para los contratos.

