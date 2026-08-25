module GeneticAlgorithms.Tests.DistanceTests

open Expecto
open GeneticAlgorithms

[<Tests>]
let hammingTests =
    testList
        "Distance.hamming"
        [ testCase "identical arrays have a distance of zero"
          <| fun _ -> Expect.equal (Distance.hamming [| 1; 0; 1 |] [| 1; 0; 1 |]) 0 "no positions should differ"

          testCase "empty arrays have a distance of zero"
          <| fun _ -> Expect.equal (Distance.hamming Array.empty<int> Array.empty<int>) 0 "empty arrays should be identical"

          testCase "counts positions containing different values"
          <| fun _ ->
              Expect.equal (Distance.hamming [| 1; 0; 1; 1 |] [| 0; 0; 1; 0 |]) 2 "two positions should differ"

          testCase "works with non-numeric values"
          <| fun _ -> Expect.equal (Distance.hamming [| 'a'; 'b'; 'c' |] [| 'a'; 'x'; 'c' |]) 1 "one character should differ"

          testCase "is symmetric"
          <| fun _ ->
              let left = [| 1; 0; 0; 1 |]
              let right = [| 0; 0; 1; 1 |]

              Expect.equal
                  (Distance.hamming left right)
                  (Distance.hamming right left)
                  "distance should not depend on argument order"

          testCase "rejects arrays of different lengths"
          <| fun _ ->
              Expect.throwsT<System.ArgumentException>
                  (fun _ -> Distance.hamming [| 1; 0 |] [| 1 |] |> ignore)
                  "Hamming distance should require arrays of equal length" ]

[<Tests>]
let jaroTests =
    testList
        "Distance.jaro"
        [ testCase "identical strings have a similarity of 1.0"
          <| fun _ -> Expect.equal (Distance.jaro "hello" "hello") 1.0 "identical strings should be perfectly similar"

          testCase "two empty strings have a similarity of 1.0"
          <| fun _ -> Expect.equal (Distance.jaro "" "") 1.0 "two empty strings should be considered identical"

          testCase "an empty string compared to a non-empty string has a similarity of 0.0"
          <| fun _ ->
              Expect.equal (Distance.jaro "" "hello") 0.0 "an empty string should have no similarity to a non-empty one"
              Expect.equal (Distance.jaro "hello" "") 0.0 "a non-empty string should have no similarity to an empty one"

          testCase "strings with no matching characters have a similarity of 0.0"
          <| fun _ -> Expect.equal (Distance.jaro "abc" "xyz") 0.0 "strings with no shared characters should have no similarity"

          testCase "is symmetric"
          <| fun _ ->
              let left = "DWAYNE"
              let right = "DUANE"

              Expect.equal (Distance.jaro left right) (Distance.jaro right left) "similarity should not depend on argument order"

          testCase "matches the well-known MARTHA/MARHTA example"
          <| fun _ ->
              let result = Distance.jaro "MARTHA" "MARHTA" |> fun value -> System.Math.Round(value, 3)

              Expect.equal result 0.944 "should match the textbook Jaro similarity value" ]
