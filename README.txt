
Skeleton Web Application based on MVC 3.0, C# 4.0 and NHibernate 3.1

Version: 1.0.0
Date:	 2011-03-24
---------------------------------------------------------------------

This is a fully functional skeleton for a web application.
It provides the following features:
- Built on C# 4.0, MVC 3.0, Razor view engine and NHibernate 3.1 supported by FluentNHibernate
- The whole solution is configurable from one XML file
  All configuration is accessible from anywhere in the code through properties from a static configuration class.
- External services can be configured in Services.xml
  There is an available example of how to implement one in SkeletonSite.Mvc.Logic (check ExampleServiceServiceProvider.cs)
- Logging solution that enabled output to multiple targets, comes with logger for debug console, log file and web application.
  Custom loggers are very easy to implement.
- NHibernate 3.1 fully implemented using FluentNHibernate.
  This allows for all configuration to be performed in code, instead of XML files.
  Object mapping is also done in code with a Fluent interface.
- Database schema creation from code, so you only have to create your object model and NHibernate can create your database.
- BaseEntity class that provides all needed CRUD operations, with support for paging and sorting.
- Multi-language support (also applied to model validation messages)
- Unobtrusive client-side model validation (see example form /Home/Login)
- A clear seperation of all code by using five projects:
  * Kernel          : Provides data access and objects, Logging, Configuration and utilities
  * Mvc	            : Mvc web application (views, css, js, images, etc)
  * Mvc.Models      : Mvc models
  * Mvc.Controllers : Mvc controllers
  * Mvc.Logic       : Web application logic

---------------------------------------------------------------------

IMPORTANT!!!
WHEN USING THE FIRST TIME:

Create an empty database and configure the connectionstring in Configuration.xml on line 10
Also modify the domain setting on line 25 and the temp path on line 27.

Now build and run the solution.
The default MVC action is set to /Home/CreateDatabase so that the database will be created on first run.

After the database creation has succeeded, 
change the default MVC action to /Home/Index in the Global.asax and run the solution again.
An example form page will now be displayed.

Call /Home/Testing to run system tests.

Known issue: cookies do not work in Chrome if you run the solution from the ASP.NET dev server (probably due to the port number)


---------------------------------------------------------------------
Created by Ruud van Falier

Email:    ruud.vanfalier@gmail.com
Twitter:  BrruuD
Blog:     http://ruuddottech.blogspot.com
LinkedIn: http://www.linkedin.com/in/ruudvanfalier