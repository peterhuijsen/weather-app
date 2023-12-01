import {baseUrl} from "@/src/services/api";

export async function getTemperature() {
	const result = await fetch(`${baseUrl}/weather/get`, {
		method: "GET",
		mode: "cors",
	});

	return parseFloat(await result.text());
}