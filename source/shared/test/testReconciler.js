var assert = require("assert");
var shared = require("../lib/index");

describe("Test reconciler", function () {
    it("Test property literal changed", function () {
        assertReconciliations({
            original: {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "Hello world"
                    }
                ]
            },
            updated: {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "It works!"
                    }
                ]
            },
            expected: [
                {
                    "type": "ObjectChanges",
                    "changes": {
                        "text": "It works!"
                    }
                }
            ]
        })
    });


    it("Test type changed inside array", function () {
        assertReconciliations({
            original: {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "Hello world"
                    }
                ]
            },
            updated: {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "Image",
                        "url": "https://msn.com"
                    }
                ]
            },
            expected: [
                {
                    "type": "ArrayChanges",
                    "property": "body",
                    "changes": [
                        {
                            "type": "Remove",
                            "index": 0
                        },
                        {
                            "type": "Add",
                            "index": 0,
                            "item": {
                                "type": "Object",
                                "props": {
                                    "type": "Image",
                                    "url": "https://msn.com"
                                }
                            }
                        }
                    ]
                }
            ]
        })
    });


    it("Test type changed inside property", function () {
        assertReconciliations({
            original: {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "Hello world",
                        "selectAction": {
                            "type": "Action.Submit",
                            "title": "Click me"
                        }
                    }
                ]
            },
            updated: {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "Hello world",
                        "selectAction": {
                            "type": "Action.OpenUrl",
                            "title": "No, click me!"
                        }
                    }
                ]
            },
            expected: [
                {
                    "type": "ObjectChanges",
                    "changes": {
                        "selectAction": {
                            "type": "Object",
                            "props": {
                                "type": "Action.OpenUrl",
                                "title": "No, click me!"
                            }
                        }
                    }
                }
            ]
        })
    });

    it("Test body added", function () {
        assertReconciliations({
            original: {
                "type": "AdaptiveCard"
            },
            updated: {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "Hello world"
                    }
                ]
            },
            expected: [
                {
                    "type": "ObjectChanges",
                    "changes": {
                        "body": {
                            "type": "Array",
                            "values": [
                                {
                                    "type": "Object",
                                    "props": {
                                        "type": "TextBlock",
                                        "text": "Hello world"
                                    }
                                }
                            ]
                        }
                    }
                }
            ]
        })
    });



    it("Test more advanced body added", function () {
        assertReconciliations({
            original: {
                "type": "AdaptiveCard"
            },
            updated: {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "Input.ChoiceSet",
                        "id": "make",
                        "style": "compact",
                        "isMultiSelect": false,
                        "choices": [
                            {
                                "title": "Mazda",
                                "value": "mazda"
                            },
                            {
                                "title": "Toyota",
                                "value": "toyota"
                            }
                        ]
                    }
                ]
            },
            expected: [
                {
                    "type": "ObjectChanges",
                    "changes": {
                        "body": {
                            "type": "Array",
                            "values": [
                                {
                                    "type": "Object",
                                    "props": {
                                        "type": "Input.ChoiceSet",
                                        "id": "make",
                                        "style": "compact",
                                        "isMultiSelect": false,
                                        "choices": {
                                            "type": "Array",
                                            "values": [
                                                {
                                                    "type": "Object",
                                                    "props": {
                                                        "title": "Mazda",
                                                        "value": "mazda"
                                                    }
                                                },
                                                {
                                                    "type": "Object",
                                                    "props": {
                                                        "title": "Toyota",
                                                        "value": "toyota"
                                                    }
                                                }
                                            ]
                                        }
                                    }
                                }
                            ]
                        }
                    }
                }
            ]
        })
    });
});

function removeIdFromItem(item) {
    if (typeof item === "object" && item !== null) {
        delete item.id;

        if (item.type == "Array" && Array.isArray(item.values)) {
            for (var i = 0; i < item.values.length; i++) {
                removeIdFromItem(item.values[i]);
            }
        } else if (item.type == "Object" && item.props) {
            for (var key in item.props) {
                removeIdFromItem(item.props[key]);
            }
        }
    }
}

function assertReconciliations(options) {
    var originalPayload = options.original;
    var updatedPayload = options.updated;
    var expectedChanges = options.expected;

    var reconciler = new shared.Reconciler(originalPayload);
    var changesAsJson = reconciler.reconcileToJson(updatedPayload);
    var changes = JSON.parse(changesAsJson);

    // Remove id fields as they'll always be random
    for (var i = 0; i < changes.length; i++) {
        delete changes[i].id;
        if (changes[i].type === "ArrayChanges") {
            var arrayChanges = changes[i].changes;
            for (var x = 0; x < arrayChanges.length; x++) {
                if (arrayChanges[x].item) {
                    removeIdFromItem(arrayChanges[x].item);
                }
            }
        } else if (changes[i].type === "ObjectChanges") {
            var objectChanges = changes[i].changes;
            for (var p in objectChanges) {
                removeIdFromItem(objectChanges[p]);
            }
        }
    }

    assert.deepStrictEqual(changes, expectedChanges, "Changes weren't equal to expected changes");
}