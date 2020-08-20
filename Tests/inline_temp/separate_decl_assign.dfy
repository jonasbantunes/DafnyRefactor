method Main() {
  var x, a: int;
  x, a := 2 + 1, 3;
  var y: int := 3;
  var z: int := y + 3*x;
  y := 10;
  assert z == 4*x;
}
