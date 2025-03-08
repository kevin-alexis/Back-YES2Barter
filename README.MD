# Nombre del Proyecto 

**YES2Barter** (Your Extra Stuff to Barter - Tus cosas extra para intercambiar)

## Problemática

Diariamente, objetos en buen estado, como ropa, muebles, electrodomésticos y dispositivos electrónicos, quedan olvidados en nuestras casas o son desechados prematuramente. De acuerdo con una investigación realizada por GreenPeace:

> “El consumo excesivo e innecesario (consumismo) es la causa del 60% de todas las emisiones globales de Gases de Efecto Invernadero (GEI). ¿Cómo acabar con el consumismo? Necesitamos consumir menos, mejor y de forma más responsable. ¿Cómo? Hay muchas opciones para hacerlo. Una de ellas es el trueque”.

En este contexto, el trueque se presenta como una alternativa ideal, ya que permite el intercambio de bienes y servicios sin utilizar dinero, facilitando la reutilización de productos excedentes y ayudando a combatir el consumismo.

A pesar de la existencia de plataformas como Facebook Marketplace o Mercado Libre, que se centran en la compra y venta de productos, no existe un sistema dedicado exclusivamente al trueque. Esto dificulta la reutilización de productos en buen estado que ya no se necesitan, demostrando la necesidad de una solución sencilla y accesible para el intercambio de bienes sobrantes.

## Integrantes
- **kevin-alexis, Bello Maldonado Kevin Alexis - 22393124, 22393124@utcancun.edu.mx**
- **Jacob0418, Coronado Cob Jose Antonio - 22393130, 22393130@utcancun.edu.mx**
- **mezaaziel06, Hernandez Meza Aziel Michell - 22393132, 22393132@utcancun.edu.mx**
- **Solrac619, Garcia Padilla Carlos Giovanny - 21393201, 21393201@utcancun.edu.mx**
- **rafa133169, Canche Monje Jose Rafael - 22394092, 22394092@utcancun.edu.mx**

## Tecnologías y Librerías Utilizadas

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


## Ejecución del Back End

1. **Clonar el Repositorio:**
```bash
git clone https://github.com/kevin-alexis/Back-YES2Barter.git
```
    
2. **Abre el Proyecto:**  
Utiliza **Visual Studio 2022** para cargar la solución.

3. **Actualiza la cadena de conexión:**
Edita el archivo `appsettings.Development.json` y actualiza la conexión a la base de datos:
```json
"DefaultConnection": "Data Source=CAMBIALO_POR_TU_SERVER_BD;Initial Catalog=yestobarter;Integrated Security=True;Trust Server Certificate=True"
```

4. **Aplicar las migraciones:**
Ejecuta el siguiente comando en la **Package Manager Console:**
```bash
update-database
```
Nota: Asegúrate de seleccionar el proyecto del repositorio como **Default Project.**

5. **Iniciar el Proyecto:**
Presiona el botón de Ejecutar en Visual Studio y selecciona la opción con HTTPS.
   
6. **Verificar:**  
Abre tu navegador y accede a la documentación Swagger: https://localhost:7257/swagger/index.html
