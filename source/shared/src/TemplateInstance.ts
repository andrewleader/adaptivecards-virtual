import * as ACTemplating from "adaptivecards-templating";
import * as helpers from "./helpers";
import _ from "lodash";

export class TemplateInstance {
    private _template: ACTemplating.Template;
    private _context: ACTemplating.EvaluationContext = new ACTemplating.EvaluationContext();
    private _currExpanded: any;

    constructor(templateObj: any, data: any) {
        this._template = new ACTemplating.Template(templateObj);
        this._context.$root = data;
        this._currExpanded = this._template.expand(this._context);
    }

    /* Returns true if transformed has changed */
    updateData(dataChanges: any) : boolean {
        this._context.$root = helpers.mergeRecursively(this._context.$root, dataChanges);

        // Expand the template
        var expanded = this._template.expand(this._context);

        if (!_.isEqual(expanded, this._currExpanded)) {
            this._currExpanded = expanded;
            return true;
        } else {
            return false;
        }
    }

    get expandedTemplate() {
        return this._currExpanded;
    }
}