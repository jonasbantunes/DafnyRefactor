
method Main()
{
  var x: int := 3;
  var y: int := 0;
  y := y + 3;
  var z: int := y + 3*x;
  {
    var x: int := 5;
    x:= x + 1;
  }
  assert z == 4*x;
}