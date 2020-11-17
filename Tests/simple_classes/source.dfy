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
        var message := "Shape was drawn at " + x + ":" + this.y;
        logger.Log(message);
    }

    method Draw(logger: Logger)
    {
        LogDrawing(logger);
        (this).LogDrawing(logger);
    }
}
