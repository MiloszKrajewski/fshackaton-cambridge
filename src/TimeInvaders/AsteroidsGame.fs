namespace TimeInvaders

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open System.IO
open System.Collections.Generic
open TimeInvaders.Item

type AsteroidsGame () as this =
    inherit Game()

    let screenWidth, screenHeight = 800.0f, 600.0f
    let graphics = new GraphicsDeviceManager(this)
    let mutable spriteBatch: SpriteBatch = null
    do 
        this.Window.Title <- "TimeInvaders"
        graphics.PreferredBackBufferWidth <- int screenWidth
        graphics.PreferredBackBufferHeight <- int screenHeight

    let loadImage (device: GraphicsDevice) file =
        let path = Path.Combine(".", file)
        use stream = File.OpenRead(path)
        let texture = Texture2D.FromStream(device, stream)
        let textureData = Array.create<Color> (texture.Width * texture.Height) Color.Transparent
        texture.GetData(textureData)
        texture

    let mutable shipTexture = null
    let mutable asteroidTexture = null
    let mutable missileTexture = null
    let mutable fontTexture = null

    let mutable crashed = false
    let mutable finished = false
    let mutable allowShooting = true

    let inside (item: Item) = 
        let x, y = item.position.X, item.position.Y
        x > -16.0f && y > -16.0f 
        && x < (screenWidth + 16.0f) 
        && y < (screenHeight + 16.0f)

    let createShip texture = { 
        position = Vector2(screenWidth / 2.0f, screenHeight / 2.0f)
        direction = 0.0f
        velocity = 0.0f
        rotation = 0.0f
        rotation_speed = 0.0f
        size = 32.0f
        texture = texture
        scale = 1.0f
    }

    let createAsteroid () =
        {
            position = Vector2(Math.rnd () * screenWidth, Math.rnd () * screenHeight)
            direction = Math.rnd () * 360.0f
            velocity = Math.rnd () + 2.0f
            rotation = 360.0f
            rotation_speed = Math.rnd () * 10.0f - 0.05f
            size = 32.0f
            texture = asteroidTexture
            scale = Math.rnd () + 0.5f
        }

    let createDebris asteroid =
        let scale = asteroid.scale * 0.75f
        let direction = asteroid.direction
        if scale < 0.5f then
            []
        else
            [ 
                { asteroid with 
                    scale = scale
                    direction = direction + 60.0f |> Math.wrap360
                    rotation_speed = Math.rnd () * 10.0f - 0.05f }
                { asteroid with 
                    scale = scale
                    direction = direction - 60.0f |> Math.wrap360
                    rotation_speed = Math.rnd () * 10.0f - 0.05f }
            ]

    let createMissile (ship: Item) =
        {
            position = ship.position
            direction = ship.direction
            velocity = 20.0f
            rotation = 0.0f
            rotation_speed = 0.0f
            size = 16.0f
            texture = missileTexture
            scale = 1.0f
        }

    let moveItem item = Item.moveItem screenWidth screenHeight item

    let mutable ship = createShip null
    let mutable asteroids = []
    let mutable missiles = []

    override this.LoadContent () =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        let load = loadImage this.GraphicsDevice

        shipTexture <- load "spaceship.png"
        asteroidTexture <- load "asteroid.png"
        missileTexture <- load "missile.png"
        fontTexture <- load "led_font.png"

        ship <- { ship with texture = shipTexture }
        asteroids <- List.init 10 (fun _ -> createAsteroid ())

    member private this.handleInput () = 
        let keyboard = Keyboard.GetState()
        let pressed k = keyboard.IsKeyDown(k)
            
        let left, right = pressed Keys.Left, pressed Keys.Right
        let direction = 
            match left, right with
            | true, false -> ship.direction - 10.0f
            | false, true -> ship.direction + 10.0f
            | _ -> ship.direction
            |> Math.wrap360

        let up, down = pressed Keys.Up, pressed Keys.Down
        let veloctiy =
            match up, down with
            | true, false -> (ship.velocity + 2.0f) |> min 10.0f
            | false, true -> (ship.velocity - 1.0f) |> max -5.0f
            | _ -> ship.velocity * 0.90f

        if (pressed Keys.Space) && (not crashed) then
            if allowShooting then
                missiles <- (createMissile ship) :: missiles
                allowShooting <- false
        else
            allowShooting <- true

        ship <- { ship with direction = direction; velocity = veloctiy }

        if pressed Keys.R then
            crashed <- false

    member private this.handleCollisions () =
        missiles <- missiles |> List.filter inside
        let hitPairs = overlaps missiles asteroids
        let missilesUsed = HashSet(hitPairs |> Seq.map fst)
        let asteroidsHit = HashSet(hitPairs |> Seq.map snd)
        missiles <- missiles |> List.filter (fun m -> missilesUsed.Contains(m) |> not)
        asteroids <- asteroids |> List.filter (fun a -> asteroidsHit.Contains(a) |> not)
        asteroids <- 
            asteroidsHit 
            |> Seq.collect createDebris 
            |> List.ofSeq
            |> List.append asteroids
        
        if asteroids |> List.exists (overlap ship) then
            crashed <- true

        if asteroids.IsEmpty then
            finished <- true

    override this.Update (gameTime) =  
        this.handleInput ()
        this.handleCollisions ()

        missiles <- missiles |> List.map moveItem
        ship <- ship |> moveItem
        asteroids <- asteroids |> List.map moveItem

    member private this.drawText (w, h) (position: Vector2) text = 
        text |> Seq.iteri (fun i c ->
            let x = position.X + (single i) * (single w)
            let y = position.Y
            let target = Rectangle(int x, int y, w, h)
            let source = Font.fontRectangle c
            source |> Option.iter (fun source ->
                spriteBatch.Draw(
                    fontTexture, target, Nullable(source), Color.White))           
        )

    override this.Draw (gameTime) =
        this.GraphicsDevice.Clear Color.Black

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied) 
        
        let draw (item: Item) =
            let center = Vector2(item.size / 2.0f, item.size / 2.0f)
            spriteBatch.Draw(
                item.texture, 
                position = Nullable(item.position),
                origin = Nullable(center),
                rotation = Math.rad (item.direction + item.rotation),
                scale = Nullable(Vector2(item.scale, item.scale)))

        asteroids |> List.iter draw
        if not crashed then draw ship
        missiles |> List.iter draw

        this.drawText (16, 24) (Vector2(480.0f, 16.0f)) (DateTime.Now.ToString())

        if crashed then 
            this.drawText (40, 58) (Vector2(100.0f, 100.0f)) "GAME OVER, MAN!"

        if finished && (not crashed) then
            this.drawText (40, 58) (Vector2(100.0f, 100.0f)) "YOU WON, GREAT!"

        spriteBatch.End()

