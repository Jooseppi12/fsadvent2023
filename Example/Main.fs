namespace Example

open System
open WebSharper
open WebSharper.JavaScript
open Validus

[<JavaScript>]
module Main =

    type PersonDto =
        {
            FirstName : string
            LastName  : string
            Email     : string
            Age       : int option
            StartDate : DateTime option
        }

        member this.PrettyPrint() =
            sprintf """
    {
        FirstName: %s
        LastName: %s
        Email: %s
        Age: %s
        StartDate: %s
    }
"""
                this.FirstName this.LastName this.Email (this.Age |> Option.defaultValue 0 |> string) (this.StartDate |> Option.defaultValue DateTime.Now |> string)

    type Name =
        {
            First : string
            Last  : string
        }

    type Person =
        {
            Name      : Name
            Email     : string
            Age       : int option
            StartDate : DateTime
        }

    module Person =
        let ofDto (dto : PersonDto) =
            // A basic validator
            let nameValidator =
                Check.String.betweenLen 3 64

            // Composing multiple validators to form complex validation rules,
            // overriding default error message (Note: "Check.WithMessage.String" as
            // opposed to "Check.String")
            let emailValidator =
                let emailPatternValidator =
                    let msg = sprintf "Please provide a valid %s"
                    Check.WithMessage.String.pattern @"[^@]+@[^\.]+\..+" msg

                ValidatorGroup(Check.String.betweenLen 8 512)
                    .And(emailPatternValidator)
                    .Build()

            // Defining a validator for an option value
            let ageValidator =
                Check.optional (Check.Int.between 1 100)

            // Defining a validator for an option value that is required
            let dateValidator =
                Check.required (Check.DateTime.greaterThan DateTime.Now)

            validate {
            let! first = nameValidator "First name" dto.FirstName
            and! last = nameValidator "Last name" dto.LastName
            and! email = emailValidator "Email address" dto.Email
            and! age = ageValidator "Age" dto.Age
            and! startDate = dateValidator "Start Date" dto.StartDate

            // Construct Person if all validators return Success
            return {
                Name = { First = first; Last = last }
                Email = email
                Age = age
                StartDate = startDate }
            }

    [<SPAEntryPoint>]
    let Main () =
        
        let scenario1, single_err =
            {
                FirstName = "John"
                LastName  = "John"
                Email     = "john@john"
                Age       = Some 15
                StartDate = Some (DateTime(2035,12,01))
            } |> fun d -> d, Person.ofDto d

        let scenario2, multi_err =
            {
                FirstName = "John"
                LastName  = "Doe"
                Email     = "john"
                Age       = None
                StartDate = Some (DateTime(1999,12,01))
            } |> fun d -> d, Person.ofDto d

        let scenario3, ok =
            {
                FirstName = "John"
                LastName  = "Doe"
                Email     = "john@doe.com"
                Age       = Some 15
                StartDate = Some (DateTime(2035,12,01))
            } |> fun d -> d, Person.ofDto d

        let resolveValidation (r: Result<Person, ValidationErrors>) =
            match r with
            | Result.Ok _ ->
                "OK"
            | Result.Error errors ->
                ValidationErrors.toList errors
                |> String.concat "\n"

        let s1, r1 = JS.Document.QuerySelector "#sample_singleerr > div:first-of-type", JS.Document.QuerySelector "#sample_singleerr > div:last-of-type"
        let s2, r2 = JS.Document.QuerySelector "#sample_multierr > div:first-of-type", JS.Document.QuerySelector "#sample_multierr > div:last-of-type"
        let s3, r3 = JS.Document.QuerySelector "#sample_ok > div:first-of-type", JS.Document.QuerySelector "#sample_ok > div:last-of-type"
        s1.TextContent <- sprintf "%A" <| scenario1.PrettyPrint().Trim([|'"'|])
        s2.TextContent <- sprintf "%A" <| scenario2.PrettyPrint().Trim([|'"'|])
        s3.TextContent <- sprintf "%A" <| scenario3.PrettyPrint().Trim([|'"'|])
        r1.TextContent <- resolveValidation single_err
        r2.TextContent <- resolveValidation multi_err
        r3.TextContent <- resolveValidation ok
