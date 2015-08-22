namespace TimeInvaders

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

module Item =
    open System

    type Item = {
        position: Vector2
        direction: float32
        velocity: float32
        rotation: float32
        rotation_speed: float32
        size: float32
        scale: float32
        texture: Texture2D
    }

    let overlap (itemA: Item) (itemB: Item) = 
        let dx = itemA.position.X - itemB.position.X |> float
        let dy = itemA.position.Y - itemB.position.Y |> float
        let d = Math.Sqrt(dx*dx + dy*dy)
        single d < (itemA.size * itemA.scale + itemB.size * itemB.scale) / 2.0f

    let overlaps listA listB =
        seq {
            for a in listA do
                for b in listB do
                    if overlap a b then
                        yield (a, b)
        }

    let moveItem screenWidth screenHeight (item: Item) = 
        let r = Math.rad (-item.direction - 180.0f) |> float
        let v = item.velocity |> float
        let x, y = float item.position.X, float item.position.Y
        let x' = x + v * Math.Sin(r) |> Math.wrap -16 (screenWidth + 16.0f)
        let y' = y + v * Math.Cos(r) |> Math.wrap -16 (screenHeight + 16.0f)
        let position = Vector2(single x', single y')
        let rotation = item.rotation + item.rotation_speed |> Math.wrap360
        { item with position = position; rotation = rotation }
