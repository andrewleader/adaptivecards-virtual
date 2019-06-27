import { Reconciler, ReconcilerChange, ReconciledLiteral } from "./Reconciler";
import { TemplateInstance } from "./TemplateInstance";

declare function onChanges(changes: string): any;
declare function onVirtualCardChanged(virtualCard: string): any;
declare function onTransformedTemplateChanged(template: string): any;
declare function onDataChanged(data: string): any;
declare function get(url: string): void;

declare var XMLHttpRequest: any;
declare function exec(script: string): void;

export class SharedRenderer {
    private _reconciler = new Reconciler({
        "type": "AdaptiveCard"
    });
    private _templateInstance?: TemplateInstance;
    private _urlCache: Map<string, any> = new Map<string, any>();

    initialize(cardJson: string, dataJson: string, cardScript: string, cardWidth: number) {
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

        this._templateInstance = new TemplateInstance(cardObj, dataObj, this);
        if (onDataChanged) {
            onDataChanged(JSON.stringify(dataObj));
        }
        if (onTransformedTemplateChanged) {
            onTransformedTemplateChanged(JSON.stringify(this._templateInstance!.expandedTemplate));
        }

        var changes = this._reconciler.reconcileToJson(this._templateInstance.expandedTemplate);
        if (onVirtualCardChanged) {
            onVirtualCardChanged(JSON.stringify(this._reconciler.reconciledCard));
        }
        onChanges(changes);

        if (cardScript.length > 0) {
            exec(cardScript);
        }
    }

    getInitialInputs(cardEl: any) {
        var answer: any = {};

        if (cardEl.type === "Input.Text" || cardEl.type === "Input.ChoiceSet") {
            answer[cardEl.id] = {
                value: ""
            };
        }

        else if (cardEl.type === "Input.Rating") {
            answer[cardEl.id] = {
                value: 0
            };
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
        var inputs: any = {};
        inputs[inputId] = JSON.parse(inputValue);

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

    updateData(data: string) {
        this.updateDataHelper(JSON.parse(data));
    }

    updateTemplate(template: string) {
        if (this._templateInstance!.updateTemplate(JSON.parse(template))) {
            if (onTransformedTemplateChanged) {
                onTransformedTemplateChanged(JSON.stringify(this._templateInstance!.expandedTemplate));
            }
            var changes = this._reconciler.reconcileToJson(this._templateInstance!.expandedTemplate);
            if (changes.length > 0) {
                if (onVirtualCardChanged) {
                    onVirtualCardChanged(JSON.stringify(this._reconciler.reconciledCard));
                }
                onChanges(changes);
            }
        }
    }

    get(url: string) {
        if (this._urlCache.has(url)) {
            return this._urlCache.get(url);
        } else {
            this._urlCache.set(url, null);
            if (get) {
                get(url);
            } else {
                try {
                    var xhttp = new XMLHttpRequest();
                    xhttp.addEventListener("load", () => {
                        this.gotHttpResponse(url, xhttp.responseText);
                    });
                    xhttp.open("GET", url);
                    xhttp.send();
                } catch (err) {
                    return JSON.stringify(err);
                }
            }
            return undefined;
        }
    }

    gotHttpResponse(url: string, response: string) {
        try {
            this._urlCache.set(url, JSON.parse(response));
        } catch (err) {
            this._urlCache.set(url, response);
        }
        this.updateData("{}");
    }

    private updateDataHelper(newData: any) {
        if (this._templateInstance!.updateData(newData)) {
            if (onTransformedTemplateChanged) {
                onTransformedTemplateChanged(JSON.stringify(this._templateInstance!.expandedTemplate));
            }
            var changes = this._reconciler.reconcileToJson(this._templateInstance!.expandedTemplate);
            if (changes.length > 0) {
                if (onVirtualCardChanged) {
                    onVirtualCardChanged(JSON.stringify(this._reconciler.reconciledCard));
                }
                onChanges(changes);
            }
        }
        if (onDataChanged) {
            onDataChanged(JSON.stringify(this._templateInstance!.data));
        }
    }

    executeAction(id: string) {
        try {
            var action = this.getById(id);
            if (action == undefined) {
                return;
            }
            switch (action.getValueTypeName()) {
                case "Action.RunScript":
                    var scriptPropVal = action.getProp("script");
                    if (scriptPropVal instanceof ReconciledLiteral) {
                        var script:string = scriptPropVal.value;
                        exec(script);
                    }
                    break;
            }
        } catch (err) {}
    }

    getData(): any {
        return this._templateInstance!.data;
    }

    private getById(id: string) {
        return this._reconciler.reconciledCardObj.getById(id);
    }
}