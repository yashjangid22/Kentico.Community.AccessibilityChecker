import React from "react";
import { Headline, HeadlineSize } from "@kentico/xperience-admin-components";

import { getScoreStyle } from "../severity";

interface ScoreBadgeProps {
    score: number;
}

export const ScoreBadge = ({ score }: ScoreBadgeProps) => {
    const style = getScoreStyle(score);
    return (
        <Headline size={HeadlineSize.S} labelColor={style.color}>
            Score: {score} / 100
        </Headline>
    );
};
