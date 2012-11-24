## Being the Worst

This folder contains additional materials for **Episode 14 – Back In The U.S.S. caR Factory Tests**:

This is part of the final version of the **Car Factory sample** within the beingtheworst.com podcast.

Technically, the code here matches the approach used by Lokad for production purposes (at the moment of writing), however the domain is far from being perfect.

* CSharp sample project covering Application Services, Domain Services, DSL code-generation, and Specifications/Unit Tests
* See "Visual Studio Settings" section on how to setup .ddd files:[ http://lokad.github.com/lokad-codedsl/](http://lokad.github.com/lokad-codedsl/)
* To execute a version of the Lokad-codedsl utility included in the sample, please launch the dsl.cmd inside of E014-car-factory-tests\sample-csharp\dsl.cmd"
* If you keep this DSL console utility running in the background it will automatically re-generate C# code in Messages.cs when you make changes and save the Messages.ddd file inside of the "Contracts" folder of the project.

The Visual Studio solution contains two projects related to Episode 14.

* **E014.Domain** - (contains the Domain and actual code for it)
* **E014.Domain.Test** - (contains the specifications, unit tests, and a console test runner that tests the Domain)

How to run it?
--------------------- 	

There are two options:

* You can open the solution and run Domain.Test as a console application (it will print specification text output to the console)

* Open the solution in Visual Studio and run unit tests (e.g. with NUnit runner or ReSharper built-in test runner)

Then watch what is printed to output for each test.

How to understand it?
---------------------

Listen to the first 14 episodes of [beingtheworst.com](http://beingtheworst.com/about) podcast, focusing on [episodes](http://beingtheworst.com/category/podcasts) 11, 12 and 14.


Subscribe to the podcast at [beingtheworst.com](http://feeds.feedburner.com/beingtheworst)
or via [iTunes](http://itunes.apple.com/us/podcast/being-the-worst/id554597082).

Follow us on twitter: [@beingtheworst](https://twitter.com/beingtheworst)

Best regards,

Kerry Street [@kcstreet](https://twitter.com/kcstreet) and Rinat Abdullin [@abdullin](https://twitter.com/abdullin)
