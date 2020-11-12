class Logger
{
    method Log(message: string)
    {
        print(message);
    }
}

class Shape
{
    var x: string;
    var y: string;

    method LogDrawing(logger: Logger)
    {
        var message := "Shape was drawn at " + this.x + ":" + this.y;
        logger.Log(message);
    }
}
