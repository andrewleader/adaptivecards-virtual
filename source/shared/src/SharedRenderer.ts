import { Reconciler, ReconcilerChange } from "./Reconciler";
import { TemplateInstance } from "./TemplateInstance";

declare function onChanges(changes: string):any;

export class SharedRenderer {
    private _reconciler = new Reconciler({
        "type": "AdaptiveCard"
    });
    private _templateInstance?: TemplateInstance;

    initialize(cardJson: string, dataJson: string, cardWidth: number) {
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

        dataObj = {
            ...dataObj,
            card: {
                width: cardWidth
            },
            inputs: this.getInitialInputs(cardObj)
        };

        this._templateInstance = new TemplateInstance(cardObj, dataObj);

        var changes = this._reconciler.reconcileToJson(this._templateInstance.expandedTemplate);
        onChanges(changes);
    }

    getInitialInputs(cardEl: any) {
        var answer: any = {};

        if (cardEl.type === "Input.Text" || cardEl.type === "Input.ChoiceSet") {
            answer[cardEl.id] = "";
        }

        for (var prop in cardEl) {
            var val = cardEl[prop];
            if (Array.isArray(val)) {
                val.forEach(item => {
                    var subanswer = this.getInitialInputs(item);
                    for (var subkey in subanswer) {
                        answer[subkey] = subanswer[subkey];
                    }
                });
            } else if (typeof val === "object" && val !== null) {
                var subanswer = this.getInitialInputs(val);
                for (var subkey in subanswer) {
                    answer[subkey] = subanswer[subkey];
                }
            }
        }

        return answer;
    }

    updateInputValue(inputId: string, inputValue: string) {
        var inputs:any = {};
        inputs[inputId] = inputValue;

        this.updateDataHelper({
            "inputs": inputs
        });
    }

    updateCardWidth(width: number) {
        this.updateDataHelper({
            "card": {
                "width": width
            }
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