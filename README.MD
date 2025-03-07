# YES2Barter

**Your Extra Stuff to Barter - Tus cosas extra para intercambiar**

## Descripción

Diariamente, objetos en buen estado, como ropa, muebles, electrodomésticos y dispositivos electrónicos, quedan olvidados en nuestras casas o son desechados prematuramente. De acuerdo con una investigación realizada por GreenPeace:

> “El consumo excesivo e innecesario (consumismo) es la causa del 60% de todas las emisiones globales de Gases de Efecto Invernadero (GEI). ¿Cómo acabar con el consumismo? Necesitamos consumir menos, mejor y de forma más responsable. ¿Cómo? Hay muchas opciones para hacerlo. Una de ellas es el trueque”.

En este contexto, el trueque se presenta como una alternativa ideal, ya que permite el intercambio de bienes y servicios sin utilizar dinero, facilitando la reutilización de productos excedentes y ayudando a combatir el consumismo.

## Problemática

A pesar de la existencia de plataformas como Facebook Marketplace o Mercado Libre, que se centran en la compra y venta de productos, no existe un sistema dedicado exclusivamente al trueque. Esto dificulta la reutilización de productos en buen estado que ya no se necesitan, demostrando la necesidad de una solución sencilla y accesible para el intercambio de bienes sobrantes.

## Integrantes

- **mezaaziel06, Hernandez Meza Aziel Michell - 22393132, 22393132@utcancun.edu.mx**

## Tecnologías y Librerías Utilizadas

### Front End

- **Vue:** Framework versátil para crear interfaces de usuario.
- **TypeScript:** Lenguaje fuertemente tipado basado en JavaScript.

#### Librerías para Front End

- **TailwindCSS:** Framework de CSS basado en utilidades.
- **Vue Router:** Facilitador de la navegación y el enrutamiento.
- **Axios:** Cliente basado en promesas para peticiones HTTP.
- **Jwt-decode:** Gestión sencilla de autenticación y extracción de datos de usuario.
- **Pinia:** Biblioteca para compartir estados entre componentes.
- **Heroicons:** Íconos SVG de alta calidad.
- **Sweetalert2:** Alternativa personalizable a los cuadros de alerta por defecto.
- **Vee-validate:** Validación de formularios en Vue con reglas y mensajes personalizados.
- **Yup:** Generador de esquemas para validación de datos.
- **Bcrypt:** Cifrado y verificación de contraseñas mediante hashing.

### Back End

- **.NET:** Plataforma de alto rendimiento y seguridad.
- **C#:** Lenguaje orientado a objetos moderno y seguro.

### Librerías para Back End

- **Mapeo de objetos**:
    - **AutoMapper**: Simplifica la asignación de objetos.
- **Acceso a datos y ORM**:
    - **Dapper**: ORM ligero para ejecutar consultas SQL y mapear resultados a objetos.
    - **Microsoft.EntityFrameworkCore.SqlServer**: Proveedor de EF Core para SQL Server.
    - **Microsoft.EntityFrameworkCore.Design**: Soporte para migraciones en tiempo de diseño.
    - **Microsoft.EntityFrameworkCore.Tools**: Administra migraciones y genera `DbContext`.
- **Autenticación y autorización**:
    - **Microsoft.AspNet.Identity.EntityFramework** / **Microsoft.AspNetCore.Identity** / **Microsoft.AspNetCore.Identity.EntityFrameworkCore**: Sistema de identidad para autenticación en ASP.NET Core.
    - **Microsoft.AspNetCore.Authentication.JwtBearer**: Middleware para autenticación con JWT.
- **HTTP y API**:
    - **Microsoft.AspNetCore.Http.Features**: Definiciones para funciones HTTP en ASP.NET Core.
    - **Swashbuckle.AspNetCore**: Generación de documentación Swagger para APIs.


### Base de datos

**SQL Server:** Microsoft SQL Server es un sistema de administración de bases de datos relacionales (RDBMS). Las aplicaciones y las herramientas se conectan a una instancia o base de datos de SQL Server y se comunican mediante Transact-SQL (T-SQL).
---

### Ejecución del Front End

1. **Clonar el Repositorio:**
    `git clone https://github.com/kevin-alexis/Front-YES2Barter.git`
    
2. **Instalar Dependencias:**
    `cd Front-YES2Barter npm install`
    
3. **Configurar Variables de Entorno:**  
    Asegúrate de establecer las variables necesarias para la conexión a servicios externos o bases de datos (según corresponda).
    
4. **Iniciar el Servidor de Desarrollo:**
    `npm run serve`
    
5. **Verificar:**  
    Abre tu navegador en  el puerto configurado para comprobar que la aplicación funcione correctamente.

#### Pasos para Ejecutar el Back End

1. **Clonar el Repositorio:**
    
    `git clone https://github.com/kevin-alexis/Front-YES2Barter.git`
    
2. **Abrir el Proyecto:**  
    Utiliza Visual Studio o tu editor preferido compatible con .NET.
3. **Restaurar Dependencias:**
    
    `dotnet restore`
    
4. **Aplicar Migraciones y Configurar la Base de Datos:**  
    Si utilizas Entity Framework, ejecuta:
    
    `dotnet ef database update`
    
5. **Iniciar el Proyecto:**
    
    `dotnet run --project WebApi`
    
    El servidor se iniciará en en el puerto configurado en `launchSettings.json`.
6. **Verificar:**  
    Accede a la URL local mediante un navegador o Postman para comprobar el funcionamiento de los endpoints.

Para más detalles sobre cómo configurar y utilizar, consulta la documentación específica en los archivos correspondientes dentro del proyecto.