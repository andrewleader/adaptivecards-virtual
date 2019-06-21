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
});

function removeIdFromItem(item) {
    if (Array.isArray(item)) {
        delete item.id;
    } else if (typeof item === "object" && item !== null) {
        delete item.id;
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