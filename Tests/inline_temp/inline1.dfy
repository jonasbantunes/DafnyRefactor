// A basic scoped test
method Main()
{
  var x: int := 2+1;
  var y: int := 0;
  y := y + 3;
  var z: int := y + 3*x;
  y := 10;
  if (x == x)
  {
    var x: int := 5;
    x:= x + 1;
  }
  assert z == 4*x;
}