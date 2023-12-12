namespace AsJavascriptLib

open WebSharper
open WebSharper.JavaScript

module Proxies =

    // Proxy for Regex.IsMatch function
    [<Proxy(typeof<System.Text.RegularExpressions.Regex>)>]
    type internal RegexProxy =
        static member IsMatch(toMatch: string, pattern: string) = RegExp(pattern).Test toMatch