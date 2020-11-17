class Node
{
    var next: Node?;
    var value: int;

    constructor()
    ensures this.next == null
    {
        this.next := null;
    }
}

method sum(x: int, y: int)
returns (res: int)
{
   res := x + y;
}

method Main()
{
    var n1 := new Node();
    var n2 := new Node();
    var n3 := new Node();
    n1.value := 1;
    n1.next := n2;
    n2.value := 2;
    n2.next := n3;
    n3.value := 3;
    n3.next := null;

    var s := n1.value + n2.value;
    n3.next := null;
    n3.value := 4;
    var res:= sum(1, s);
}
