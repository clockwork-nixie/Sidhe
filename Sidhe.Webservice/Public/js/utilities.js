export default class Utilities {
    static formatJson(json, indent = 3) {
        let result = '';
        let depth = 0;
        let isEscaped = false;
        let isQuoted = false;

        for (let i = 0; i < json.length; ++i) {
            const character = json[i];

            if (character === '\\' && !isEscaped) {
                isEscaped = true;
                continue;
            } else if (character === '"' && !isEscaped) {
                isQuoted = !isQuoted;
            } else if (!isQuoted && !isEscaped) {
                if (character === '}' || character === ']') {
                    depth = depth > 0? --depth: 0;
                    result += '\r\n' + Array(depth * indent).fill('\xa0').join('');
                }
            }
            result += character;

            if (!isQuoted && !isEscaped) {
                if (character === '{' || character === '[') {
                    result += '\r\n';
                    ++depth;
                } else if (character === ',') {
                    result += '\r\n';
                } else if (character !== '\n') {
                    continue;
                }
                result += Array(depth * indent).fill('\xa0').join('');
            }
        }
        return result;
    }
}