import uuid from "uuid";

enum ValueType {
    Array,
    Object,
    Literal
}

function getType(value: any): ValueType {
    if (Array.isArray(value)) {
        return ValueType.Array;
    } else if (value !== null && typeof value == "object") {
        return ValueType.Object;
    } else {
        return ValueType.Literal;
    }
}

export abstract class ReconciledBase {
    private _id: string;

    constructor() {
        this._id = uuid.v4();
    }

    get id() { return this._id; }

    abstract get valueType(): ValueType;

    abstract toJsonObj(): any;

    static create(value: any): ReconciledBase {
        switch (getType(value)) {
            case ValueType.Array:
                return new ReconciledArray(value);

            case ValueType.Object:
                return new ReconciledObject(value);

            case ValueType.Literal:
                return new ReconciledLiteral(value);

            default:
                throw new Error("Unknown type");
        }
    }

    abstract getById(id: string): ReconciledObject | undefined;
}

export class ReconciledObject extends ReconciledBase {
    private _props: Map<string, ReconciledBase> = new Map<string, ReconciledBase>();

    constructor(originalObj: any) {
        super();

        for (var p in originalObj) {
            this._props.set(p, ReconciledBase.create(originalObj[p]));
        }
    }

    getProp(propName: string) {
        return this._props.get(propName);
    }

    setProp(propName: string, value: ReconciledBase) {
        this._props.set(propName, value);
    }

    get valueType() { return ValueType.Object; }

    /* The object's type name, like "TextBlock" */
    getValueTypeName(): string | undefined {
        var typePropVal = this.getProp("type");
        if (typePropVal === undefined) {
            return undefined;
        }
        if (typePropVal instanceof ReconciledLiteral) {
            return typePropVal.value;
        }
        return undefined;
    }

    toJsonObj() {
        var jsonProps: any = {};

        this._props.forEach((value, propName) => {
            jsonProps[propName] = value.toJsonObj();
        });

        return {
            "type": "Object",
            "id": this.id,
            "props": jsonProps
        };
    }

    getById(id: string): ReconciledObject | undefined {
        if (this.id == id) {
            return this;
        }

        var answer: ReconciledObject | undefined;

        this._props.forEach(value => {
            if (answer == undefined) {
                answer = value.getById(id);
            }
        });

        return answer;
    }
}

export class ReconciledLiteral extends ReconciledBase {
    private _value: any;

    constructor(originalValue: any) {
        super();
        this._value = originalValue;
    }

    get value() { return this._value; }

    get valueType() { return ValueType.Literal; }

    toJsonObj() {
        return this.value;
    }

    getById(id: string): ReconciledObject | undefined {
        return undefined;
    }
}

export class ReconciledArray extends ReconciledBase {
    private _values: ReconciledBase[] = [];

    constructor(originalValues: any[]) {
        super();

        originalValues.forEach(value => {
            this._values.push(ReconciledBase.create(value));
        });
    }

    get values() { return this._values; }
    get valueType() { return ValueType.Array; }

    add(index: number, value: ReconciledBase) {
        this._values.splice(index, 0, value);
    }

    remove(index: number) {
        this._values.splice(index, 1);
    }

    swap(index: number, newValue: ReconciledBase) {
        this._values.splice(index, 1, newValue);
    }

    toJsonObj() {
        var jsonValues: any[] = [];

        this._values.forEach(value => {
            jsonValues.push(value.toJsonObj());
        });

        return {
            "type": "Array",
            "id": this.id,
            "values": jsonValues
        };
    }

    getById(id: string): ReconciledObject | undefined {
        for (var i = 0; i < this._values.length; i++) {
            var answer = this._values[i].getById(id);
            if (answer) {
                return answer;
            }
        }
        return undefined;
    }
}

export abstract class ReconcilerChange {
    abstract toJsonObj(): any;
}

export abstract class ReconcilerArrayChange {
    private _index: number;

    constructor(index: number) {
        this._index = index;
    }

    get index() { return this._index; }
}

export class ReconcilerArrayAddChange extends ReconcilerArrayChange {
    private _item: ReconciledBase;

    constructor(index: number, item: ReconciledBase) {
        super(index);
        this._item = item;
    }
    
    get item() { return this._item; }
}

export class ReconcilerArrayRemoveChange extends ReconcilerArrayChange {
}

export class ReconcilerArrayChanges extends ReconcilerChange {
    private _objectId: string;
    private _changes: ReconcilerArrayChange[] = [];
    private _property: string;

    constructor(objectId: string, property: string) {
        super();
        this._objectId = objectId;
        this._property = property;
    }

    add(index: number, item: ReconciledBase) {
        this._changes.push(new ReconcilerArrayAddChange(index, item));
    }

    remove(index: number) {
        this._changes.push(new ReconcilerArrayRemoveChange(index));
    }

    get id() { return this._objectId; }
    get changes() { return this._changes; }

    hasChanges() {
        return this._changes.length > 0;
    }

    toJsonObj() {
        var jsonChanges: any[] = [];
        this._changes.forEach(change => {
            if (change instanceof ReconcilerArrayAddChange) {
                jsonChanges.push({
                    "type": "Add",
                    "index": change.index,
                    "item": change.item.toJsonObj()
                });
            } else if (change instanceof ReconcilerArrayRemoveChange) {
                jsonChanges.push({
                    "type": "Remove",
                    "index": change.index
                });
            }
        });

        return {
            "type": "ArrayChanges",
            "id": this.id,
            "property": this._property,
            "changes": jsonChanges
        };
    }
}

export class ReconcilerUpdatedItemProperties extends ReconcilerChange {
    private _itemId: string;
    private _updatedProperties: Map<string, ReconciledBase | undefined> = new Map<string, ReconciledBase | undefined>();

    constructor(itemId: string) {
        super();
        this._itemId = itemId;
    }

    get id() { return this._itemId; }
    get updatedProperties() { return this._updatedProperties; }

    addUpdatedProperty(propName: string, value: ReconciledBase) {
        this._updatedProperties.set(propName, value);
    }

    removeProperty(propName: string) {
        this._updatedProperties.set(propName, undefined);
    }

    hasChanges() {
        return this._updatedProperties.size > 0;
    }

    toJsonObj() {
        var jsonChanges: any = {};

        this._updatedProperties.forEach((value, propName) => {
            if (value === undefined) {
                jsonChanges[propName] = null;
            } else {
                jsonChanges[propName] = value.toJsonObj();
            }
        });

        var id: string | undefined = undefined;
        try {
            id = this.id;
        } catch (err) {}

        return {
            "type": "ObjectChanges",
            "id": id,
            "changes": jsonChanges
        };
    }
}

export class Reconciler {
    private _currReconciledCard: ReconciledObject;

    constructor(card: any) {
        this._currReconciledCard = new ReconciledObject(card);
    }

    reconcileToJson(newCard: any) : string {
        var changes = this.reconcile(newCard);
        var jsonChanges: any[] = [];
        changes.forEach(change => {
            jsonChanges.push(change.toJsonObj());
        });
        return JSON.stringify(jsonChanges);
    }

    reconcile(newCard: any): ReconcilerChange[] {
        var changes: ReconcilerChange[] = [];
        this.reconcileObjectChanges(this._currReconciledCard, newCard, changes);
        // this._currCard = newCard;
        return changes;
    }

    get reconciledCard() {
        return this._currReconciledCard.toJsonObj();
    }

    get reconciledCardObj() {
        return this._currReconciledCard;
    }

    private reconcileObjectChanges(_currReconciledObject: ReconciledObject, newObject: any, changes: ReconcilerChange[]) {
        var updatedItemChanges = new ReconcilerUpdatedItemProperties(_currReconciledObject.id);

        for (var p in newObject) {
            var existingPropValue = _currReconciledObject.getProp(p);
            var newPropValue = newObject[p];

            // If property doesn't exist or it changed type or it's a literal
            if (existingPropValue === undefined
                || this.shouldSwapValue(existingPropValue, newPropValue)) {
                // We treat it as a set property
                var reconciledValue = ReconciledBase.create(newPropValue);
                _currReconciledObject.setProp(p, reconciledValue);
                updatedItemChanges.addUpdatedProperty(p, reconciledValue);
            } else if (existingPropValue instanceof ReconciledObject) {
                this.reconcileObjectChanges(existingPropValue, newPropValue, changes);
            } else if (existingPropValue instanceof ReconciledArray) {
                this.reconcileArrayChanges(_currReconciledObject.id, p, existingPropValue, newPropValue, changes);
            }
        }

        if (updatedItemChanges.hasChanges()) {
            changes.push(updatedItemChanges);
        }
    }

    private shouldSwapValue(existing: ReconciledBase, newValue: any) {
        return this.didChangeType(existing, newValue)
            || (existing instanceof ReconciledLiteral && existing.value !== newValue);
    }

    private didChangeType(existing: ReconciledBase, newValue: any) {
        return getType(newValue) != existing.valueType
            || (existing instanceof ReconciledObject && existing.getValueTypeName() !== newValue.type);
    }

    private reconcileArrayChanges(parentObjId: string, parentProperty: string, _currReconciledArray: ReconciledArray, newArray: any[], changes: ReconcilerChange[]) {
        var arrayChanges = new ReconcilerArrayChanges(parentObjId, parentProperty);

        var maxLength = Math.max(_currReconciledArray.values.length, newArray.length);

        for (var i = 0; i < maxLength; i++) {
            var existingValue = undefined;
            var newValue = undefined;

            if (i < _currReconciledArray.values.length) {
                existingValue = _currReconciledArray.values[i];
            }
            if (i < newArray.length) {
                newValue = newArray[i];
            }

            if (existingValue === undefined) {
                // Was added
                var newReconciledValue = ReconciledBase.create(newValue);
                _currReconciledArray.add(i, newReconciledValue);
                arrayChanges.add(i, newReconciledValue);
            } else if (newValue === undefined) {
                // Was deleted
                _currReconciledArray.remove(i);
                arrayChanges.remove(i);
                i--;
                maxLength--;
            } else if (this.shouldSwapValue(existingValue, newValue)) {
                // Since need to swap, delete and then add
                var newReconciledValue = ReconciledBase.create(newValue);
                _currReconciledArray.swap(i, newReconciledValue);
                arrayChanges.remove(i);
                arrayChanges.add(i, newReconciledValue);
            } else if (existingValue instanceof ReconciledObject) {
                // Can update the existing object
                this.reconcileObjectChanges(existingValue, newValue, changes);
            }
        }

        if (arrayChanges.hasChanges()) {
            changes.push(arrayChanges);
        }
    }
}