namespace TimeInvaders

module Font =
    open Microsoft.Xna.Framework

    let private fontMap = 
        [
            "!    '[]*+,- /"
            "0123456789|  ="
            "   ABCDEFGHIJK"
            "LMNOPQRSTUVWXY"
            "Z \ ^_`       "
            ""
            "      :"
        ]
        |> Seq.mapi (fun y l -> (y, l |> Seq.mapi (fun x c -> (x, c))))
        |> Seq.collect (fun (y, l) -> l |> Seq.map (fun (x, c) -> (c, (y, x))))
        |> Seq.filter (fun (c, _) -> c <> ' ')
        |> Map.ofSeq

    let fontRectangle c =
        let w, h = 300.0 / 14.0, 202.0 / 7.0
        fontMap.TryFind c
        |> Option.map (fun (y, x) -> 
            let x = w * float x
            let y = h * float y
            Rectangle(int x + 2, int y, int w, int h))
