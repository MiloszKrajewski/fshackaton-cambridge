namespace TimeInvaders

module Math =
    open System

    let inline rad angle =
        (float angle) * Math.PI / 180.0 |> single

    let inline wrap lo hi v = 
        if single v < single lo then single hi 
        else if single v > single hi then single lo 
        else single v

    let rec wrap360 v =
        if v < 0.0f then wrap360 (360.0f + v)
        elif v >= 360.0f then wrap360 (v - 360.0f)
        else v

    let rnd = 
        let r = new Random()
        fun () -> r.NextDouble() |> single
