/*
* Recursively merge properties of two objects
* https://stackoverflow.com/questions/171251/how-can-i-merge-properties-of-two-javascript-objects-dynamically
*/
export function mergeRecursively(obj1: any, obj2: any) : any {
    var answer: any = { ...obj1 };

    for (var p in obj2) {
        if (obj2.hasOwnProperty(p)) {
            try {
                // Property in destination object set; update its value.
                if (obj2[p].constructor == Object) {
                    answer[p] = mergeRecursively(answer[p], obj2[p]);
                } else {
                    answer[p] = obj2[p];
                }

            } catch (e) {
                // Property in destination object not set; create it and set its value.
                answer[p] = obj2[p];
            }
        }
    }

    return answer;
}