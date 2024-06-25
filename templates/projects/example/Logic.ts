interface Item {
    color: number[];
    height: number;
}

export default class Model {
    items: Item[] = [];

    addItem(): void {
        this.items.push({
            color: [Math.random(), Math.random(), Math.random(), 1],
            height: (Math.random() + 1.0) * 80
        });
    }
}
