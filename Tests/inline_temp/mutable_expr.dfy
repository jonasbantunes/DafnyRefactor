method Main() {
  var x: int := 2 + 1;
  var y: int := 3;
  var z: int := y + 3*x;
  y := 10;
  assert z == 4*x;
}
