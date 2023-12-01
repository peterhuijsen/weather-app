import Title from "@/components/general/Title";
import {getTemperature} from "@/src/services/temperature";
import {useEffect, useState} from "react";

export default function WeatherPrediction() {
	const [data, setData] = useState<string | undefined>()

	useEffect(() => {
		const fetchData = async () => {
			if (data !== undefined)
				return

			const response = await getTemperature();
			setData(response.toString());
		};

		fetchData()
	}, []);

	return (
		<Title>
			It is {data ?? "..."} degrees outside.
		</Title>
	)
}