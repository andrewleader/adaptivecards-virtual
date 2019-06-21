import { Reconciler, ReconcilerChange } from "./Reconciler";
import { TemplateInstance } from "./TemplateInstance";

declare function onChanges(changes: string):any;

export class SharedRenderer {
    private _reconciler = new Reconciler({
        "type": "AdaptiveCard"
    });
    private _templateInstance?: TemplateInstance;

    initialize(cardJson: string, dataJson: string) {
        var cardObj;
        try {
            cardObj = JSON.parse(cardJson);
        } catch (err) {
            throw new Error("Card JSON was invalid JSON");
        }
        var dataObj;
        try {
            dataObj = JSON.parse(dataJson);
        } catch (err) {
            throw new Error("Data JSON was invalid JSON");
        }

        this._templateInstance = new TemplateInstance(cardObj, dataObj);

        var changes = this._reconciler.reconcileToJson(this._templateInstance.expandedTemplate);
        onChanges(changes);
    }

    updateInputValue(inputId: string, inputValue: string) {
        var inputs:any = {};
        inputs[inputId] = inputValue;

        this.updateDataHelper({
            "inputs": inputs
        });
    }

    private updateDataHelper(newData: any) {
        if (this._templateInstance!.updateData(newData)) {
            var changes = this._reconciler.reconcileToJson(this._templateInstance!.expandedTemplate);
            if (changes.length > 0) {
                onChanges(changes);
            }
        }
    }
}