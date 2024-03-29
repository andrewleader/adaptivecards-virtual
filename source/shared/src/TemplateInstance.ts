import * as ACTemplating from "adaptivecards-templating";
import * as helpers from "./helpers";
import isEqual from "deep-equal";
import { SharedRenderer } from "./SharedRenderer";

declare var XMLHttpRequest: any;

export class TemplateInstance {
    private _originalTemplate: any;
    private _template: ACTemplating.Template;
    private _context: ACTemplating.EvaluationContext = new ACTemplating.EvaluationContext();
    private _currExpanded: any;
    private _renderer: SharedRenderer;

    constructor(templateObj: any, data: any, renderer: SharedRenderer) {
        this._renderer = renderer;
        this._originalTemplate = templateObj;
        this._template = new ACTemplating.Template(templateObj);
        this._context.$root = data;

        this._context.registerFunction("get", (url: string) =>
        {
            return this._renderer.get(url);
        });

        this._currExpanded = this._template.expand(this._context);
    }

    /* Returns true if transformed has changed */
    updateData(dataChanges: any) : boolean {
        // We have to re-create the template object, otherwise re-evaluating $when statements doesn't work
        this._template = new ACTemplating.Template(this._originalTemplate);
        this._context.$root = helpers.mergeRecursively(this._context.$root, dataChanges);

        // Expand the template
        var expanded = this._template.expand(this._context);

        if (!isEqual(expanded, this._currExpanded)) {
            this._currExpanded = expanded;
            return true;
        } else {
            return false;
        }
    }

    updateTemplate(templateObj: any) : boolean {
        this._originalTemplate = templateObj;
        return this.updateData({});
    }

    get expandedTemplate() {
        return this._currExpanded;
    }

    get data() {
        return this._context.$root;
    }
}