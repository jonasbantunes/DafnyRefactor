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
    var logger: Logger;

    constructor(logger: Logger){
        this.logger := logger;
    }

    method LogDrawing()
    {
        var message := "Shape was drawn at " + x + ":" + this.y;
        logger.Log(message);
        this.logger.Log(message);
    }

    method Draw()
    {
        LogDrawing();
        (this).LogDrawing();
    }
}
