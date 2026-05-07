# Reglas del proyecto: SSF.Interop.SIIFNacion.API

## Contexto general

Este proyecto es una API .NET orientada a integración con SIIF Nación.
La arquitectura sigue un enfoque por capas y patrón CQRS.
Se usan Queries, Handlers, DTOs y Services para orquestar las operaciones.
Los controladores deben ser ligeros y delegar la lógica a la capa de aplicación.

## Reglas de arquitectura

- Mantener separación clara de responsabilidades.
- Los Controllers solo reciben la solicitud HTTP, validan lo mínimo y delegan al flujo de aplicación.
- La lógica de negocio y orquestación debe ir en Queries/Commands y sus Handlers.
- Las integraciones externas con SIIF deben implementarse en Services.
- Los DTOs deben representar claramente contratos de entrada y salida.
- No agregar lógica de negocio compleja dentro de Controllers.
- No mezclar lógica de transporte HTTP con lógica de dominio.
- Reutilizar patrones existentes del proyecto antes de proponer nuevas estructuras.

## Convenciones de nombres

- Los objetos de transferencia deben terminar en `Dto`.
- Las consultas deben terminar en `Query`.
- Los manejadores deben terminar en `Handler`.
- Los servicios de integración deben tener nombres claros terminados en `Service`.
- Los métodos asíncronos deben terminar en `Async`.

## Reglas para integraciones SIIF

- Toda integración con SIIF debe pasar por servicios dedicados.
- Reutilizar el cliente/base existente para llamadas HTTP antes de crear uno nuevo.
- Si existe un método genérico como `PostAsync<TRequest, TResponse>`, debe reutilizarse.
- Los headers del request SIIF deben construirse usando DTOs específicos, por ejemplo `SiifRequestHeaderDto`.
- El body del request SIIF debe construirse con DTOs separados y explícitos.
- El manejo de errores debe ser consistente y trazable.
- Toda integración debe incluir logging útil para diagnóstico, evitando exponer secretos.
- No hardcodear URLs, credenciales ni identificadores sensibles.
- Toda configuración debe salir de `appsettings`, variables de entorno o configuración centralizada.

## Reglas de código

- Preferir código claro sobre código demasiado compacto.
- Mantener métodos pequeños y con una única responsabilidad.
- Validar nulos y entradas inválidas cuando aplique.
- Evitar duplicación de lógica.
- Si ya existe un patrón implementado para un endpoint SIIF, seguir el mismo patrón.
- No inventar nuevas abstracciones sin necesidad real.
- Mantener consistencia con el estilo actual del proyecto.

## Manejo de errores y observabilidad

- Usar logs estructurados cuando sea posible.
- Registrar contexto técnico suficiente para soporte y trazabilidad.
- No registrar tokens, contraseñas, hashes sensibles ni secretos completos.
- En errores de integración, incluir el contexto funcional del endpoint invocado.
- Propagar excepciones o respuestas controladas según el patrón ya definido en el proyecto.

## Pruebas

- Cuando se generen pruebas, usar el framework de pruebas ya presente en la solución.
- Mockear dependencias externas.
- Cubrir casos felices, validaciones y errores de integración.
- No depender de servicios externos reales en pruebas unitarias.

## Instrucciones para generación de código con IA

- Antes de generar código nuevo, revisar cómo ya está implementado `ConsultarCdp`.
- Usar `ConsultarCdpQuery`, su Handler y su Service como patrón base para nuevos endpoints.
- Cuando se cree una nueva integración SIIF, generar:
  - DTO de request
  - DTO de response
  - Query o Command
  - Handler
  - Método del Service
  - Registro de dependencias si aplica
  - Pruebas unitarias si el proyecto las maneja
- Si falta contexto, preferir seguir el patrón existente del repositorio en lugar de inventar uno nuevo.

## Seguridad

- No exponer datos sensibles en logs.
- No sugerir secretos embebidos en código.
- No deshabilitar validaciones de seguridad sin justificación explícita.
- Respetar el manejo de autenticación y hash definido por la integración.

## Preferencias de estilo para este proyecto

- Responder y generar código en C# idiomático.
- Mantener compatibilidad con la versión de .NET usada por el proyecto.
- Priorizar legibilidad, mantenibilidad y trazabilidad.
- Si se propone refactor, hacerlo incremental y compatible con el diseño existente.