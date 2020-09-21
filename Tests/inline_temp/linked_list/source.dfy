class Node_q
{
    var next: Node_q?;
    var value: int;

    constructor()
    ensures this.next == null
    {
        this.next := null;
    }
}

method Main()
{
    var n1 := new Node_q();
    var n2 := new Node_q();
    var n3 := new Node_q();
    n1.value := 1;
    n1.next := n2;
    n2.value := 2;
    n2.next := n3;
    n3.value := 3;
    n3.next := null;

    var expr := n1.next.next;
    var anotherNode := new Node_q();
    anotherNode.value := 44;
    anotherNode.next := null;

    assert expr != null;
}
