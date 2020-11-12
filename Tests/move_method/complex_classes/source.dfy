class Logger
{
    constructor()
    {
    }

    method Log(message: string)
    {
        print(message);
    }
}

class Shape
{
    var x: string;
    var y: string;

    constructor(x: string, y: string)
    {
        this.x := x;
        this.y := y;
    }

    method LogDrawing(logger: Logger)
    {
        var message := "Shape was drawn at " + x + ":" + this.y;
        logger.Log(message);
    }

    method Draw(logger: Logger)
    {
        LogDrawing(logger);
        (this).LogDrawing(logger);
    }
}

method Main()
{
    var shape := new Shape("1", "3");
    var logger := new Logger();
    shape.LogDrawing(logger);
}
