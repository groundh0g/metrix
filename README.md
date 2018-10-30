![Build Status](https://ci.appveyor.com/api/projects/status/github/groundh0g/metrix?svg=true)

# metrix

Tinkering with `IHttpModule` to create a custom HTTP module that measures certain metrics during request processing.

## Goals and Objectives

- [X] Create a new IHttpModule
- [X] Log collected page metrics to the page output
- [X] Measure:
  - [X] Total time processing page
  - [X] Overhead of the custom handler
  - [X] Size of the response (in bytes)
  - [X] Average, min, and max responses seen
- [ ] Provide script to install the custom module to all applications on IIS
- [X] Provide support for CI
  - [X] Vendor Appveyor (for Windows support)
  - [X] Include automated tests
  - [X] Include build status badge on README

## Roadmap

- [ ] Group metrics buy RawUrl
- [ ] Add API layer to allow sites to explicitly log timings
- [ ] Add config options for logging modes (NLog, EvenViewer, DB, ...)
- [ ] Keep last N frames, to support charting
- [ ] Add a slick UI with charts, using [Google Analytics](https://analytics.google.com/) as a model

## How to Use

To see the custom module in action, simply open the solution and run the MetrixWeb project from within Visual Studio 2017. This will launch the development version of IIS and you should see the collected data in the footer of whatever page you visit. The tests are in MetrixTests, and the core logic is in a class library named MetrixLib.

If you'd like to use this module on your IIS application instance, ensure the following section appears in your web.config.

~~~
  <system.web>
    <httpModules>
      <add name="MetrixHttpModule" type="MetrixWeb.MetrixHttpModule" />
    </httpModules>
    ...
  </system.web>
~~~

## Known Issues

**Failing / Ignored Test:** The measure of the response size in bytes appears to be working in the module itself, but the plumbing for HttpApplication and related classes is complex and hard to mock properly. The count of bytes is always `0` in tests. This needs to be corrected.

## Noteworthy

This application and related components was developed in the JetBrains Rider IDE for MacOS. Simply stated, this application should work on Windows, Linux, and MacOS -- Where Mono is used for the latter two.