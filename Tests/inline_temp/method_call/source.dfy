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

method printNode(node: Node)
modifies node;
{
    print(node.value);
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

    var expr := n1.next.next;
    var anotherNode := new Node();
    anotherNode.value := 44;
    anotherNode.next := null;
    
    printNode(anotherNode);
}
