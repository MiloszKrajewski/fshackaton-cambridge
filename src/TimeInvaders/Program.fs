namespace TimeInvaders

module Program =

    [<EntryPoint>]
    let main argv = 
        use game = new AsteroidsGame()
        game.Run()
        0
